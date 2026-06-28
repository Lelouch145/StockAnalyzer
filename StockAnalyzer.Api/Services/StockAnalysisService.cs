using System.Diagnostics.Eventing.Reader;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using StockAnalyzer.Api.Models;

namespace StockAnalyzer.Api.Services;


public class Analyze
{
    // Samling av metoden för att skriva ut analysen
    public AnalyzeValue? StockAnalyse(StockResult result, List<StockHistory> list)
    {
        AnalyzeValue analyze = new AnalyzeValue();
        List<string> reasons = new List<string>();
        analyze.Ticker = result.Ticker;
        analyze.CreatedAt = result.CreatedAt;
        if (list.Count == 0)
        {
            return null;
        }
        var oneDayScore = CalculateOneDayScore(result);

        var ma30Score = CalculateMovingScore(result, list, 30, reasons);

        var ma50Score = CalculateMovingScore(result, list, 50, reasons);
        int score = oneDayScore + ma30Score + ma50Score;

        analyze.Signal = DetermineSignal(score);
        analyze.RiskLevel = DetermineRiskLevel(result);
        AddSignalReason(analyze.Signal, reasons);

        analyze.Reason = string.Join(" | ", reasons);
        return analyze;


    }
    // Bestämmer signalen beroende på vilken poäng aktien får
    public string DetermineSignal(int score)
    {
        int bullishThreshold = 3;
        int bearishThreshold = -3;
        string signal;
        if (score >= bullishThreshold)
        {
            signal = "Bullish";
        }
        else if (score <= bearishThreshold)
        {
            signal = "Bearish";
        }
        else
        {
            signal = "Neutral";
        }
        return signal;
    }
    // Beroende på procentändringen från StockResult så skrivs risknivån
    public string DetermineRiskLevel(StockResult result)
    {
        string riskLevel;
        int highRiskThreshold = 4;
        int mediumRiskThreshold = 2;
        if (result.ChangePercent >= highRiskThreshold || result.ChangePercent <= -highRiskThreshold)
        {
            riskLevel = "High";
        }
        else if ((result.ChangePercent >= mediumRiskThreshold && result.ChangePercent <= highRiskThreshold) ||
        (result.ChangePercent <= -mediumRiskThreshold && result.ChangePercent >= -highRiskThreshold))
        {
            riskLevel = "Medium";
        }
        else
        {
            riskLevel = "Low";
        }
        return riskLevel;
    }
    // Lägger till förkklaring bredvid signalen
    public void AddSignalReason(string signal, List<string> reasons)
    {
        if (signal == "Bullish")
        {
            reasons.Add("Bullish → Multiple indicators show positive momentum.");
        }
        else if (signal == "Bearish")
        {
            reasons.Add("Bearish → Multiple indicators show negative momentum.");
        }
        else if (signal == "Neutral")
        {
            reasons.Add("Neutral → Mixed or weak signals.");
        }
        
    }
    
    public decimal? CalculateMovingAverage(List<StockHistory> list, int period)
    {
        if (list.Count < period)
        {
            return null;
        }
        var days = list.Take(period);
        var getClose = days.Select(t => t.Close);
        var average = getClose.Average();
        return average;


    }

    public decimal? CalculateMovingTrend(List<StockHistory> list, int period)
    {
        if (list.Count < period)
        {
            return null;
        }
        var latestClose = list[0].Close;
        var previousDay = list[period - 1].Close;
        if (previousDay == 0)
        {
            return null;
        }
        var procentChange = (latestClose - previousDay) / previousDay * 100;
        return procentChange;
    }
    private int CalculateMovingScore(StockResult result, List<StockHistory> list, int period, List<string> reasons)
    {
        int score = 0;
        decimal? movingAverage = CalculateMovingAverage(list, period);
        decimal? movingTrend = CalculateMovingTrend(list, period);
        var stronglyTrendThreshold = 10;
        var normalTrendThreshold = 5;
        if (movingAverage == null)
        {
            return 0;
        }
        if(movingTrend == null)
        {
            return 0;
        }
        if (result.Price > movingAverage)
        {
            score += 2;
            reasons.Add($"Price is above {period}-day trend");
        }
        else if (result.Price < movingAverage)
        {
            score -= 2;
            reasons.Add($"Price is below {period}-day trend");
        }
        if (movingTrend >= stronglyTrendThreshold)
        {
            score += 2;
            reasons.Add($"{period} days trend is strongly positive");
        }
        else if (movingTrend >= normalTrendThreshold && movingTrend < stronglyTrendThreshold)
        {
            score += 1;
            reasons.Add($"{period} days trend is positive");
        }
        else if (movingTrend <= -stronglyTrendThreshold)
        {
            score -= 2;
            reasons.Add($"{period} days trend is strongly negative");
        }
        else if (movingTrend <= -normalTrendThreshold && movingTrend > -stronglyTrendThreshold)
        {
            score -= 1;
            reasons.Add($"{period} days trend is negative");
        }

        return score;
    }
    public int CalculateOneDayScore(StockResult result)
    {
        int score = 0;
        int highScoreChangeThreshold = 2;
        int lowScoreChangeThreshold = 0;
        if (result.ChangePercent >= highScoreChangeThreshold)
        {
            score += 2;
        }
        else if (result.ChangePercent > lowScoreChangeThreshold && result.ChangePercent < highScoreChangeThreshold)
        {
            score += 1;
        }
        else if (result.ChangePercent <= -highScoreChangeThreshold)
        {
            score -= 2;
        }
        else if (result.ChangePercent < lowScoreChangeThreshold && result.ChangePercent > -highScoreChangeThreshold)
        {
            score -= 1;
        }
        if (result.Price > result.Open)
        {
            score += 1;
        }
        else if (result.Price < result.Open)
        {
            score -= 1;
        }
        decimal range = result.High - result.Low;
        decimal range25 = range * 0.25m;
        if (result.Price >= result.High - range25)
        {
            score += 1;
        }
        else if (result.Price <= result.Low + range25)
        {
            score -= 1;
        }
        if (result.Price > result.PreviousClose)
        {
            score += 1;
        }
        else if (result.Price < result.PreviousClose)
        {
            score -= 1;
        }
        return score;
    }
}