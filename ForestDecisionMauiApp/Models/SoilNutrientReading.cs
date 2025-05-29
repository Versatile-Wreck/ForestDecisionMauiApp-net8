// Models/SoilNutrientReading.cs
using System;

namespace ForestDecisionMauiApp.Models
{
    public class SoilNutrientReading
    {
        public string ReadingID { get; set; } // 唯一读数ID
        public string SiteID { get; set; } // 关联的监测点ID
        public DateTime Timestamp { get; set; }
        public double? NitrogenTotal { get; set; }
        public double? PhosphorusTotal { get; set; }
        public double? PotassiumTotal { get; set; }
        public double? NitrogenAvailable { get; set; }
        public double? PhosphorusAvailable { get; set; }
        public double? PotassiumAvailable { get; set; }
        public NutrientUnit Unit { get; set; } = NutrientUnit.MilligramsPerKilogram; // 默认单位
        public string DataSource { get; set; } // e.g., "SensorA", "LabSample"

        public override string ToString()
        {
            return $"Reading ID: {ReadingID} for Site {SiteID} at {Timestamp.ToString("yyyy-MM-dd HH:mm")} - N_avail: {NitrogenAvailable}, P_avail: {PhosphorusAvailable}, K_avail: {PotassiumAvailable} ({Unit})";
        }
    }
}