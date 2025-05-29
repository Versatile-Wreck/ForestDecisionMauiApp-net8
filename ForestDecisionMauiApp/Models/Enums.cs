// Enums.cs
namespace ForestDecisionMauiApp.Models
{
    public enum AgeClass
    {
        Undefined, // 未定义
        Age_0_5,   // 0-5年
        Age_6_10,  // 6-10年
        Age_11_15, // 11-15年
        Age_16_20  // 16-20年
    }

    public enum PlotType
    {
        Undefined,         // 未定义
        StandardForestPlot, // 标准林分
        RunoffPlot,         // 径流小区
        MixedPlantingPlot   // 混交林样地
    }

    public enum UserRole
    {
        Administrator,
        Researcher,
        Operator
    }

    public enum NutrientUnit
    {
        MilligramsPerKilogram, // mg/kg
        Percent              // %
    }
}