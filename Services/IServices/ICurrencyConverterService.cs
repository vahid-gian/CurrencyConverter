using Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.DTOs;

namespace Services.IServices
{
    public interface ICurrencyConverterService
    {
        /// <summary>
        /// Clears any prior configuration.
        /// </summary>
        Task ClearConfigurationAsync();
        /// <summary>
        /// Updates the configuration. Rates are inserted or replaced internally.
        /// </summary>
        Task UpdateConfigurationAsync(IEnumerable<Configuration> conversionRates);

        /// <summary>
        /// Converts the specified amount to the desired currency.
        /// </summary>
        Task<ConversionResponseDto> ConvertAsync(ConversionInput input);

        void CalculateAllPossibleConversions();
    }
}
