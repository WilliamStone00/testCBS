using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.LocalizationService
{
    public class LocalizationService : BaseService
    {
        private readonly ApiCallerHelper _systemConfigApiHelper;

        public LocalizationService()
        {
             _systemConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["SystemConfigurationBaseUrl"].ToString());
        }
        public async Task<List<StringValues>> GetValueOption(string switch_on, string Id = "0")
        {
            List<StringValues> selectListItems = new List<StringValues>();
            var LocationDto = await this.GetLocationInfo();
            switch (switch_on)
            {
                case "REGION":
                    {

                        foreach (var item in LocationDto.Regions)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "DIVISION":
                    {
                        var modelList = LocationDto.Divisions.Where(x => x.RegionId == Id).ToList();
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "SUBDIVISION":
                    {
                        var modelList = LocationDto.Subdivisions.Where(x => x.DivisionId == Id).ToList();
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "TOWN":
                    {
                        var modelList = LocationDto.Towns.Where(x => x.SubdivisionId == Id).ToList();
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;


                default: break;
            }

            return selectListItems;
        }
        public async Task<List<StringValues>> GetLocationValueOption(string switch_on, string Id = "0")
        {
            switch_on = Id;
            List<StringValues> selectListItems = new List<StringValues>();
            var LocationDto = await this.GetLocationInfo();
            switch (switch_on)
            {
                case "REGION":
                    {

                        foreach (var item in LocationDto.Regions)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "DIVISION":
                    {
                        var modelList = LocationDto.Divisions;
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "SUBDIVISION":
                    {
                        var modelList = LocationDto.Subdivisions;
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "TOWN":
                    {
                        var modelList = LocationDto.Towns;
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;


                default: break;
            }

            return selectListItems;
        }
        public async Task<LocationDto> GetLocationInfo()
        {
            try
            {
                var couApiResponse = await _systemConfigApiHelper.GetAsync<ResponseObject<LocationDto>>(APICallHelper.GetlocationInforUrl);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new LocationDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
    }
}
