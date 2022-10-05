using Data.Context;
using Data.DbModels;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Services.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        
        private readonly ApplicationDbContext _context;

        public CurrencyConverterService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task ClearConfigurationAsync()
        {
            _context.Configurations.RemoveRange(_context.Configurations);
            await _context.SaveChangesAsync();
        }

        public async Task<ConversionResponseDto> ConvertAsync(ConversionInput input)
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(x =>
                x.FromCurrency.Equals(input.FromCurrency) && x.ToCurrency.Equals(input.ToCurrency));
            if (config is not null)
                return new ConversionResponseDto()
                {
                    FromCurrency = input.FromCurrency,
                    ToCurrency = input.ToCurrency,
                    Rate = config.Rate,
                    Path = config.Path,
                    Amount = input.Amount,
                    ConvertedAmount = input.Amount * config.Rate
                };

            return null;
        }

        public async Task UpdateConfigurationAsync(IEnumerable<Configuration> conversionRates)
        {
            var configurations = conversionRates as Configuration[] ?? conversionRates.ToArray();
            await _context.Configurations.AddRangeAsync(configurations);
            await _context.SaveChangesAsync();
            CalculateAllPossibleConversions();
            CalculateAllReverseConversions();
        }

        public void CalculateAllPossibleConversions()
        {
            var configurations = _context.Configurations.ToList();
            foreach (var config in configurations)
            {
                RecursiveCalculateCalculationConversion(config, null);
            }
        }

        public void CalculateAllReverseConversions()
        {
            var configurations = _context.Configurations.ToList();
            foreach (var config in configurations)
            {
                CalculateReverseConversions(config);
            }

            _context.SaveChanges();
        }

        private void RecursiveCalculateCalculationConversion(Configuration currency, Configuration prevCurrency)
        {
            var configs = _context.Configurations.Where(x=> x.FromCurrency.Equals(currency.ToCurrency)).ToList();
            foreach (var config in configs)
            {
                var existingConfig = _context.Configurations.SingleOrDefault(x =>
                    x.FromCurrency.Equals(currency.FromCurrency) && x.ToCurrency.Equals(config.ToCurrency));
                if (existingConfig is null)
                {
                    var newConfig = new Configuration()
                    {
                        FromCurrency = prevCurrency is null ? currency.FromCurrency : prevCurrency.FromCurrency,
                        ToCurrency = config.ToCurrency,
                        Rate = prevCurrency is null ? currency.Rate * config.Rate : prevCurrency.Rate * config.Rate,
                        Path = string.Concat(prevCurrency is null ? "" : string.Concat(prevCurrency.FromCurrency,"=>"), currency.FromCurrency, "=>", config.FromCurrency, "=>", config.ToCurrency)
                    };
                    _context.Configurations.Add(newConfig);

                    RecursiveCalculateCalculationConversion(config, newConfig);
                }
            }

            _context.SaveChanges();
        }

        private void CalculateReverseConversions(Configuration config)
        {
            var existingConfig = _context.Configurations.SingleOrDefault(x =>
                x.FromCurrency.Equals(config.ToCurrency) && x.ToCurrency.Equals(config.FromCurrency));
            if (existingConfig is null)
            {
                var newConfig = new Configuration()
                {
                    FromCurrency = config.ToCurrency,
                    ToCurrency = config.FromCurrency,
                    Rate = 1/config.Rate,
                    Path = config.Path?.Replace("=>","<=")
                };
                _context.Configurations.Add(newConfig);
            }
        }

        
    }
}
