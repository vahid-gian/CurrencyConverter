using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data.DbModels;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class ConfigurationsController : Controller
    {
        private readonly IMapper _mapper;

        private readonly ICurrencyConverterService _currencyConverterService;
        public ConfigurationsController(ICurrencyConverterService currencyConverterService, IMapper mapper)
        {
            _currencyConverterService = currencyConverterService;
            _mapper = mapper;
        }

        [HttpPost("UpdateConfiguration")]
        public async Task<ActionResult> UpdateConfigurationAsync([FromBody] IEnumerable<ConfigurationDto> conversionRates)
        {
            IEnumerable<Configuration> configs = _mapper.Map<IEnumerable<Configuration>>(conversionRates);
            await _currencyConverterService.UpdateConfigurationAsync(configs);
            return Ok();
        }

        [HttpPost("ClearConfiguration")]
        public async Task<ActionResult> ClearConfigurationAsync()
        {
            await _currencyConverterService.ClearConfigurationAsync();
            return Ok();
        }

        [HttpPost("Convert")]
        public async Task<ConversionResponseDto> Convert([FromBody] ConversionInput input)
        {
            return await _currencyConverterService.ConvertAsync(input);
        }
    }
}
