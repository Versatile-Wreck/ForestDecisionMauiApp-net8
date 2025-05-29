// Services/DecisionService.cs
using System.Collections.Generic;
using ForestDecisionMauiApp.Models; // 引用我们的数据模型
using System; // For DateTime

namespace ForestDecisionMauiApp.Services
{
    public class DecisionService
    {
        // 在实际应用中，这些阈值和规则可能来自配置文件、数据库，或者更复杂的模型计算
        // 这里我们使用简化的、硬编码的规则作为示例

        public List<DecisionRecommendation> GenerateRecommendations(MonitoringSite site, SoilNutrientReading latestReading)
        {
            var recommendations = new List<DecisionRecommendation>();

            if (site == null || latestReading == null)
            {
                recommendations.Add(new DecisionRecommendation
                {
                    SiteID = site?.SiteID ?? "N/A",
                    Basis = "数据不完整",
                    RecommendationText = "无法生成决策建议，因为监测点或最新的养分读数数据缺失。",
                    Severity = RecommendationSeverity.Warning
                });
                return recommendations;
            }

            // 示例决策逻辑：基于速效氮 (NitrogenAvailable)
            if (latestReading.NitrogenAvailable.HasValue)
            {
                double nitrogen = latestReading.NitrogenAvailable.Value;
                string basis = $"当前速效氮含量: {nitrogen} {latestReading.Unit}";
                DecisionRecommendation recommendation = null;

                // 针对不同林龄组的简单规则示例
                switch (site.AgeClass)
                {
                    case AgeClass.Age_0_5: // 幼龄林
                        if (nitrogen < 100) // 假设幼龄林速效氮阈值为100
                        {
                            recommendation = new DecisionRecommendation
                            {
                                Basis = basis + " (阈值 < 100)",
                                RecommendationText = "幼龄林 (0-5年): 速效氮含量偏低，建议适量补充氮肥以促进生长。",
                                Severity = RecommendationSeverity.Suggestion,
                                RecommendedAction = "补充氮肥"
                            };
                        }
                        else if (nitrogen > 200) // 假设上限为200
                        {
                            recommendation = new DecisionRecommendation
                            {
                                Basis = basis + " (阈值 > 200)",
                                RecommendationText = "幼龄林 (0-5年): 速效氮含量偏高，注意监测，避免过量。",
                                Severity = RecommendationSeverity.Warning,
                                RecommendedAction = "监测氮含量"
                            };
                        }
                        break;
                    case AgeClass.Age_6_10:
                        if (nitrogen < 80) // 假设阈值调整为80
                        {
                            recommendation = new DecisionRecommendation
                            {
                                Basis = basis + " (阈值 < 80)",
                                RecommendationText = "中龄林 (6-10年): 速效氮含量偏低，考虑补充氮肥。",
                                Severity = RecommendationSeverity.Suggestion,
                                RecommendedAction = "补充氮肥"
                            };
                        }
                        break;
                    // 可以为其他林龄组添加更多规则...
                    default:
                        recommendation = new DecisionRecommendation
                        {
                            Basis = basis,
                            RecommendationText = $"林龄组 {site.AgeClass}: 速效氮含量为 {nitrogen} {latestReading.Unit}。请根据具体模型评估。",
                            Severity = RecommendationSeverity.Info
                        };
                        break;
                }
                if (recommendation != null)
                {
                    recommendation.SiteID = site.SiteID;
                    recommendations.Add(recommendation);
                }
            }
            else
            {
                recommendations.Add(new DecisionRecommendation
                {
                    SiteID = site.SiteID,
                    Basis = "速效氮数据缺失",
                    RecommendationText = "最新的养分读数中缺少速效氮数据，无法评估氮营养状况。",
                    Severity = RecommendationSeverity.Warning
                });
            }

            // 示例决策逻辑：基于速效磷 (PhosphorusAvailable)
            if (latestReading.PhosphorusAvailable.HasValue)
            {
                double phosphorus = latestReading.PhosphorusAvailable.Value;
                // 简单示例：假设所有林龄的磷适宜范围是 30-80
                if (phosphorus < 30)
                {
                    recommendations.Add(new DecisionRecommendation
                    {
                        SiteID = site.SiteID,
                        Basis = $"当前速效磷: {phosphorus} {latestReading.Unit} (阈值 < 30)",
                        RecommendationText = "速效磷含量偏低，建议补充磷肥。",
                        Severity = RecommendationSeverity.Suggestion,
                        RecommendedAction = "补充磷肥"
                    });
                }
                else if (phosphorus > 80)
                {
                    recommendations.Add(new DecisionRecommendation
                    {
                        SiteID = site.SiteID,
                        Basis = $"当前速效磷: {phosphorus} {latestReading.Unit} (阈值 > 80)",
                        RecommendationText = "速效磷含量偏高，注意监测。",
                        Severity = RecommendationSeverity.Warning
                    });
                }
            }
            // 可以为速效钾 (PotassiumAvailable) 等添加类似规则

            // 如果没有任何特定问题，可以给一个通用信息
            if (recommendations.Count == 0)
            {
                recommendations.Add(new DecisionRecommendation
                {
                    SiteID = site.SiteID,
                    Basis = "各项指标在一般检查范围内或数据不足。",
                    RecommendationText = "当前养分数据显示正常或部分数据缺失，建议结合完整模型进行综合评估。",
                    Severity = RecommendationSeverity.Info
                });
            }

            return recommendations;
        }

        // TODO: 未来这里可以替换为调用更复杂的模型，
        // 例如: private FuzzyLogicModel _fuzzyModel;
        //       private RegressionModel _nitrogenRegression;
        // public DecisionService(FuzzyLogicModel fuzzyModel, RegressionModel nitrogenRegression) { ... }
    }
}