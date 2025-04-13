using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Session
{
    using CBS.FrontDesk.Data.Entity.Config;
    using CBS.FrontDesk.Helper;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;

    public class LocationAggregateService
    {
        private const string SessionKey = "LocationAggregate";

        public Task<LocationAggregate> GetOrLoadLocationDataAsync(Aggregrate aggregrate)
        {
            if (HttpContext.Current.Session[SessionKey] is LocationAggregate cached)
                return Task.FromResult(cached);

            try
            {
                var agg = aggregrate;
                var locationData = new LocationAggregate
                {
                    Countries = agg.Countries ?? new List<Country>(),
                    Regions = agg.Regions ?? new List<Region>(),
                    Divisions = agg.Divisions ?? new List<Division>(),
                    SubDivisions = agg.Subdivisions ?? new List<SubDivision>(),
                    Towns = agg.Towns ?? new List<Town>()
                };

                HttpContext.Current.Session[SessionKey] = locationData;
                return Task.FromResult(locationData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void ClearLocationCache() => HttpContext.Current.Session.Remove(SessionKey);

        public List<Country> GetCountries() =>
            (HttpContext.Current.Session[SessionKey] as LocationAggregate)?.Countries ?? new List<Country>();

        public List<Region> GetRegions() =>
            (HttpContext.Current.Session[SessionKey] as LocationAggregate)?.Regions ?? new List<Region>();

        public List<Division> GetDivisions() =>
            (HttpContext.Current.Session[SessionKey] as LocationAggregate)?.Divisions ?? new List<Division>();

        public List<SubDivision> GetSubDivisions() =>
            (HttpContext.Current.Session[SessionKey] as LocationAggregate)?.SubDivisions ?? new List<SubDivision>();

        public List<Town> GetTowns() =>
            (HttpContext.Current.Session[SessionKey] as LocationAggregate)?.Towns ?? new List<Town>();

        // ✅ NEW: Filtering methods
        public List<Region> GetRegionsByCountry(string countryId)
        {
            return GetRegions().Where(r => r.CountryId == countryId).ToList();
        }

        public List<Division> GetDivisionsByRegion(string regionId)
        {
            return GetDivisions().Where(d => d.RegionId == regionId).ToList();
        }

        public List<SubDivision> GetSubDivisionsByDivision(string divisionId)
        {
            return GetSubDivisions().Where(s => s.DivisionId == divisionId).ToList();
        }

        public List<Town> GetTownsBySubDivision(string subDivisionId)
        {
            return GetTowns().Where(t => t.SubdivisionId == subDivisionId).ToList();
        }
    }

}
