// Models/MonitoringSite.cs
using System.Collections.Generic;

namespace ForestDecisionMauiApp.Models
{
    public class MonitoringSite
    {
        public string SiteID { get; set; }
        public string LocationDescription { get; set; }
        public AgeClass AgeClass { get; set; }
        public int SiteIndex { get; set; } // 立地指数
        public PlotType PlotType { get; set; }
        public double? AreaHectares { get; set; } // 面积（公顷），可空

        // 在实际应用中，这可能是一个更复杂的对象或通过ID关联
        public List<SoilNutrientReading> Readings { get; set; }

        public MonitoringSite()
        {
            Readings = new List<SoilNutrientReading>();
        }

        public override string ToString()
        {
            return $"Site ID: {SiteID}, Location: {LocationDescription}, Age Class: {AgeClass}, Site Index: {SiteIndex}, Type: {PlotType}";
        }
    }
}