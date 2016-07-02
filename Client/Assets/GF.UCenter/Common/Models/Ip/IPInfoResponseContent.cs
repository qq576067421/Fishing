namespace GF.UCenter.Common.Portable.Models.Ip
{
    using System.Runtime.Serialization;

    [DataContract]
    public class IPInfoResponseContent
    {
        // {"code":0,"data":{"country":"\u9999\u6e2f","country_id":"HK","area":"","area_id":"","region":"\u9999\u6e2f\u7279\u522b\u884c\u653f\u533a","region_id":"HK_01","city":"","city_id":"","county":"","county_id":"","isp":"","isp_id":"-1","ip":"23.99.99.89"}}

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "country_id")]
        public string CountryId { get; set; }

        [DataMember(Name = "area")]
        public string Area { get; set; }

        [DataMember(Name = "area_id")]
        public string AreaId { get; set; }

        [DataMember(Name = "region")]
        public string Region { get; set; }

        [DataMember(Name = "region_id")]
        public string RegionId { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "city_id")]
        public string CityId { get; set; }

        [DataMember(Name = "county")]
        public string County { get; set; }

        [DataMember(Name = "county_id")]
        public string CountyId { get; set; }

        [DataMember(Name = "isp")]
        public string Isp { get; set; }

        [DataMember(Name = "isp_id")]
        public string IspId { get; set; }

        [DataMember(Name = "ip")]
        public string IP { get; set; }
    }
}
