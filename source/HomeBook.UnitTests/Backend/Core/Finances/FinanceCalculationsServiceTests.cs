using System.Globalization;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Finances;
using HomeBook.Backend.Module.Finances.Services;
using HomeBook.UnitTests.TestCore.Helper;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HomeBook.UnitTests.Backend.Core.Finances;

[TestFixture]
public class FinanceCalculationsServiceTests
{
    private CancellationToken _cancellationToken;
    private IDateTimeProvider _dateTimeProvider;
    private FinanceCalculationsService _instance;

    [SetUp]
    public void SetUp()
    {
        _cancellationToken = CancellationToken.None;
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _instance = new FinanceCalculationsService(_dateTimeProvider,
            NullLogger<FinanceCalculationsService>.Instance);
    }

    [Test]
    public void CalculateSavings_WithStraightSumAndTargetSimpleRate_Returns()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);
        var targetAmount = 1_000;
        var targetDate = now.AddMonths(5);

        // Act
        var result = _instance.CalculateSavings(targetAmount,
            targetDate);

        // Assert
        result.ShouldNotBeNull();
        result.MonthsNeeded.ShouldBe((short)5);
        result.MonthlyPayment.ShouldBe(200);
        result.Amounts.Length.ShouldBe(5);
        result.Interests.Length.ShouldBe(0);

        result.Amounts[0].ShouldBe(200);
        result.Amounts[1].ShouldBe(400);
        result.Amounts[2].ShouldBe(600);
        result.Amounts[3].ShouldBe(800);
        result.Amounts[4].ShouldBe(1_000);
    }

    [Test]
    public void CalculateSavings_WithOddSumAndTargetSimpleRate_Returns()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);
        var targetAmount = 1_397;
        var targetDate = now.AddMonths(12);

        // Act
        var result = _instance.CalculateSavings(targetAmount,
            targetDate);

        // Assert
        result.ShouldNotBeNull();
        result.MonthsNeeded.ShouldBe((short)12);
        result.MonthlyPayment.ShouldBe(120);
        result.Amounts.Length.ShouldBe(12);
        result.Interests.Length.ShouldBe(0);

        result.Amounts[0].ShouldBe(120);
        result.Amounts[1].ShouldBe(240);
        result.Amounts[2].ShouldBe(360);
        result.Amounts[3].ShouldBe(480);
        result.Amounts[4].ShouldBe(600);
        result.Amounts[5].ShouldBe(720);
        result.Amounts[6].ShouldBe(840);
        result.Amounts[7].ShouldBe(960);
        result.Amounts[8].ShouldBe(1_080);
        result.Amounts[9].ShouldBe(1_200);
        result.Amounts[10].ShouldBe(1_320);
        result.Amounts[11].ShouldBe(1_440);
    }

    [Test]
    public void CalculateSavings_WithOddSumAndWithoutTargetSimpleRate_Returns()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);
        var targetAmount = 1_397;
        var targetDate = now.AddMonths(12);

        // Act
        var result = _instance.CalculateSavings(targetAmount,
            targetDate,
            false);

        // Assert
        result.ShouldNotBeNull();
        result.MonthsNeeded.ShouldBe((short)12);
        result.MonthlyPayment.ShouldBe((decimal)116.4, 1);
        result.Amounts.Length.ShouldBe(12);
        result.Interests.Length.ShouldBe(0);

        result.Amounts[0].ShouldBe((decimal)116.4, 1);
        result.Amounts[1].ShouldBe((decimal)232.8, 1);
        result.Amounts[2].ShouldBe((decimal)349.2, 1);
        result.Amounts[3].ShouldBe((decimal)465.6, 1);
        result.Amounts[4].ShouldBe((decimal)582.0, 1);
        result.Amounts[5].ShouldBe((decimal)698.5, 1);
        result.Amounts[6].ShouldBe((decimal)814.9, 1);
        result.Amounts[7].ShouldBe((decimal)931.3, 1);
        result.Amounts[8].ShouldBe((decimal)1047.7, 1);
        result.Amounts[9].ShouldBe((decimal)1164.1, 1);
        result.Amounts[10].ShouldBe((decimal)1280.5, 1);
        result.Amounts[11].ShouldBe((decimal)1397.0, 1);
    }

    [TestCase(7_000,
        18,
        2.08,
        385,
        18,
        18,
        1,
        1,
        "331.76,670.42,1016.13,1369.02,1729.26,2096.98,2472.36,2855.55,3246.70,3645.99,4053.59,4469.67,4894.39,5327.96,5770.54,6222.33,6683.51,7154.29",
        "6.76,13.66,20.70,27.90,35.24,42.73,50.38,58.19,66.16,74.29,82.60,91.07,99.73,108.56,117.58,126.79,136.18,145.78")]
    [TestCase(1_000,
        6,
        0,
        170,
        6,
        6,
        1,
        1,
        "170,340,510,680,850,1020",
        "0,0,0,0,0,0")]
    [TestCase(1_200,
        12,
        0.5,
        100,
        12,
        12,
        1,
        1,
        "100.50,201.50,303.01,405.03,507.55,610.59,714.14,818.21,922.80,1027.92,1133.56,1239.72",
        "0.50,1.00,1.51,2.02,2.53,3.04,3.55,4.07,4.59,5.11,5.64,6.17")]
    [TestCase(5_000,
        6,
        1.2,
        835,
        6,
        6,
        1,
        1,
        "819.72,1649.28,2488.79,3338.37,4198.15,5068.25",
        "9.72,19.56,29.51,39.59,49.78,60.10")]
    [TestCase(10_000,
        6,
        0.3,
        1670,
        6,
        6,
        1,
        1,
        "1659.96,3324.91,4994.85,6669.80,8349.77,10034.79",
        "4.96,9.94,14.94,19.95,24.97,30.01")]
    [TestCase(20_000,
        6,
        0.15,
        3335,
        6,
        6,
        1,
        1,
        "3329.99,6664.97,10004.95,13349.95,16699.96,20055.00",
        "4.99,9.98,14.98,19.99,25.01,30.04")]
    public void CalculateMonthlySavings_WithDifferentSettings_Returns(decimal targetAmount,
        int targetAdditionalMonthsFromNow,
        decimal interestRate,
        decimal expectedMonthlyPayment,
        short expectedMonthsNeeded,
        int expectedListLength,
        decimal amountExpectedTolerance,
        decimal interestsExpectedTolerance,
        string expectedAmountsCsv,
        string expectedInterestsCsv)
    {
        // Arrange
        var now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);
        var targetDate = now.AddMonths(targetAdditionalMonthsFromNow);
        decimal[] expectedAmounts = expectedAmountsCsv.ToDecimalArray(new CultureInfo("en-US"));
        decimal[] expectedInterests = expectedInterestsCsv.ToDecimalArray(new CultureInfo("en-US"));

        // Act
        var result = _instance.CalculateMonthlySavings(targetAmount,
            targetDate,
            interestRate);

        // Assert
        result.ShouldNotBeNull();
        string amountCsv =
            string.Join(",", result.Amounts.Select(a => a.ToString(CultureInfo.GetCultureInfo("en-US"))));
        string interestCsv = string.Join(",",
            result.Interests.Select(i => i.ToString(CultureInfo.GetCultureInfo("en-US"))));
        result.MonthlyPayment.ShouldBe(expectedMonthlyPayment);
        result.MonthsNeeded.ShouldBe(expectedMonthsNeeded);
        // result.Amounts.Length.ShouldBe(expectedListLength);
        // result.Interests.Length.ShouldBe(expectedListLength);
        // result.Amounts.ShouldBe(expectedAmounts, amountExpectedTolerance);
        // result.Interests.ShouldBe(expectedInterests, interestsExpectedTolerance);
    }

    [TestCase(1_000,
        18,
        0,
        60,
        18,
        18,
        1,
        1,
        "60,120,180,240,300,360,420,480,540,600,660,720,780,840,900,960,1020,1080",
        "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0")]
    [TestCase(5_000,
        60,
        2.0,
        85,
        60,
        60,
        1,
        1,
        "85,170,255,340,425,510,595,680,765,850,935,1040.40,1125.40,1210.40,1295.40,1380.40,1465.40,1550.40,1635.40,1720.40,1805.40,1890.40,1975.40,2101.61,2186.61,2271.61,2356.61,2441.61,2526.61,2611.61,2696.61,2781.61,2866.61,2951.61,3036.61,3184.04,3269.04,3354.04,3439.04,3524.04,3609.04,3694.04,3779.04,3864.04,3949.04,4034.04,4119.04,4288.12,4373.12,4458.12,4543.12,4628.12,4713.12,4798.12,4883.12,4968.12,5053.12,5138.12,5223.12,5414.28",
        "0,0,0,0,0,0,0,0,0,0,0,20.40,0,0,0,0,0,0,0,0,0,0,0,41.21,0,0,0,0,0,0,0,0,0,0,0,62.43,0,0,0,0,0,0,0,0,0,0,0,84.08,0,0,0,0,0,0,0,0,0,0,0,106.16")]
    [TestCase(10_000,
        120,
        3.5,
        85,
        120,
        120,
        1,
        1,
        "85,170,255,340,425,510,595,680,765,850,935,1055.70,1140.70,1225.70,1310.70,1395.70,1480.70,1565.70,1650.70,1735.70,1820.70,1905.70,1990.70,2148.35,2233.35,2318.35,2403.35,2488.35,2573.35,2658.35,2743.35,2828.35,2913.35,2998.35,3083.35,3279.24,3364.24,3449.24,3534.24,3619.24,3704.24,3789.24,3874.24,3959.24,4044.24,4129.24,4214.24,4449.72,4534.72,4619.72,4704.72,4789.72,4874.72,4959.72,5044.72,5129.72,5214.72,5299.72,5384.72,5661.16,5746.16,5831.16,5916.16,6001.16,6086.16,6171.16,6256.16,6341.16,6426.16,6511.16,6596.16,6915.00,7000.00,7085.00,7170.00,7255.00,7340.00,7425.00,7510.00,7595.00,7680.00,7765.00,7850.00,8212.72,8297.72,8382.72,8467.72,8552.72,8637.72,8722.72,8807.72,8892.72,8977.72,9062.72,9147.72,9555.87,9640.87,9725.87,9810.87,9895.87,9980.87,10065.87,10150.87,10235.87,10320.87,10405.87,10490.87,10946.02,11031.02,11116.02,11201.02,11286.02,11371.02,11456.02,11541.02,11626.02,11711.02,11796.02,11881.02,12384.83",
        "0,0,0,0,0,0,0,0,0,0,0,35.70,0,0,0,0,0,0,0,0,0,0,0,72.65,0,0,0,0,0,0,0,0,0,0,0,110.89,0,0,0,0,0,0,0,0,0,0,0,150.47,0,0,0,0,0,0,0,0,0,0,0,191.44,0,0,0,0,0,0,0,0,0,0,0,233.84,0,0,0,0,0,0,0,0,0,0,0,277.72,0,0,0,0,0,0,0,0,0,0,0,323.15,0,0,0,0,0,0,0,0,0,0,0,370.16,0,0,0,0,0,0,0,0,0,0,0,418.81")]
    [TestCase(20_000,
        180,
        4.2,
        115,
        180,
        180,
        1,
        1,
        "115,230,345,460,575,690,805,920,1035,1150,1265,1437.96,1552.96,1667.96,1782.96,1897.96,2012.96,2127.96,2242.96,2357.96,2472.96,2587.96,2702.96,2936.31,3051.31,3166.31,3281.31,3396.31,3511.31,3626.31,3741.31,3856.31,3971.31,4086.31,4201.31,4497.60,4612.60,4727.60,4842.60,4957.60,5072.60,5187.60,5302.60,5417.60,5532.60,5647.60,5762.60,6124.46,6239.46,6354.46,6469.46,6584.46,6699.46,6814.46,6929.46,7044.46,7159.46,7274.46,7389.46,7819.65,7934.65,8049.65,8164.65,8279.65,8394.65,8509.65,8624.65,8739.65,8854.65,8969.65,9084.65,9586.03,9701.03,9816.03,9931.03,10046.03,10161.03,10276.03,10391.03,10506.03,10621.03,10736.03,10851.03,11426.60,11541.60,11656.60,11771.60,11886.60,12001.60,12116.60,12231.60,12346.60,12461.60,12576.60,12691.60,13344.48,13459.48,13574.48,13689.48,13804.48,13919.48,14034.48,14149.48,14264.48,14379.48,14494.48,14609.48,15342.91,15457.91,15572.91,15687.91,15802.91,15917.91,16032.91,16147.91,16262.91,16377.91,16492.91,16607.91,17425.27,17540.27,17655.27,17770.27,17885.27,18000.27,18115.27,18230.27,18345.27,18460.27,18575.27,18690.27,19595.09,19710.09,19825.09,19940.09,20055.09,20170.09,20285.09,20400.09,20515.09,20630.09,20745.09,20860.09,21856.05,21971.05,22086.05,22201.05,22316.05,22431.05,22546.05,22661.05,22776.05,22891.05,23006.05,23121.05,24211.96,24326.96,24441.96,24556.96,24671.96,24786.96,24901.96,25016.96,25131.96,25246.96,25361.96,25476.96,26666.82,26781.82,26896.82,27011.82,27126.82,27241.82,27356.82,27471.82,27586.82,27701.82,27816.82,27931.82,29224.79",
        "0,0,0,0,0,0,0,0,0,0,0,57.96,0,0,0,0,0,0,0,0,0,0,0,118.35,0,0,0,0,0,0,0,0,0,0,0,181.29,0,0,0,0,0,0,0,0,0,0,0,246.86,0,0,0,0,0,0,0,0,0,0,0,315.19,0,0,0,0,0,0,0,0,0,0,0,386.39,0,0,0,0,0,0,0,0,0,0,0,460.57,0,0,0,0,0,0,0,0,0,0,0,537.88,0,0,0,0,0,0,0,0,0,0,0,618.43,0,0,0,0,0,0,0,0,0,0,0,702.36,0,0,0,0,0,0,0,0,0,0,0,789.82,0,0,0,0,0,0,0,0,0,0,0,880.95,0,0,0,0,0,0,0,0,0,0,0,975.91,0,0,0,0,0,0,0,0,0,0,0,1074.86,0,0,0,0,0,0,0,0,0,0,0,1177.97")]
    [TestCase(100_000,
        240,
        5.0,
        420,
        240,
        240,
        1,
        1,
        "420,840,1260,1680,2100,2520,2940,3360,3780,4200,4620,5292.00,5712.00,6132.00,6552.00,6972.00,7392.00,7812.00,8232.00,8652.00,9072.00,9492.00,9912.00,10848.60,11268.60,11688.60,12108.60,12528.60,12948.60,13368.60,13788.60,14208.60,14628.60,15048.60,15468.60,16683.03,17103.03,17523.03,17943.03,18363.03,18783.03,19203.03,19623.03,20043.03,20463.03,20883.03,21303.03,22809.18,23229.18,23649.18,24069.18,24489.18,24909.18,25329.18,25749.18,26169.18,26589.18,27009.18,27429.18,29241.64,29661.64,30081.64,30501.64,30921.64,31341.64,31761.64,32181.64,32601.64,33021.64,33441.64,33861.64,35995.72,36415.72,36835.72,37255.72,37675.72,38095.72,38515.72,38935.72,39355.72,39775.72,40195.72,40615.72,43087.51,43507.51,43927.51,44347.51,44767.51,45187.51,45607.51,46027.51,46447.51,46867.51,47287.51,47707.51,50533.88,50953.88,51373.88,51793.88,52213.88,52633.88,53053.88,53473.88,53893.88,54313.88,54733.88,55153.88,58352.58,58772.58,59192.58,59612.58,60032.58,60452.58,60872.58,61292.58,61712.58,62132.58,62552.58,62972.58,66562.21,66982.21,67402.21,67822.21,68242.21,68662.21,69082.21,69502.21,69922.21,70342.21,70762.21,71182.21,75182.32,75602.32,76022.32,76442.32,76862.32,77282.32,77702.32,78122.32,78542.32,78962.32,79382.32,79802.32,84233.43,84653.43,85073.43,85493.43,85913.43,86333.43,86753.43,87173.43,87593.43,88013.43,88433.43,88853.43,93737.11,94157.11,94577.11,94997.11,95417.11,95837.11,96257.11,96677.11,97097.11,97517.11,97937.11,98357.11,103715.96,104135.96,104555.96,104975.96,105395.96,105815.96,106235.96,106655.96,107075.96,107495.96,107915.96,108335.96,114193.76,114613.76,115033.76,115453.76,115873.76,116293.76,116713.76,117133.76,117553.76,117973.76,118393.76,118813.76,125195.45,125615.45,126035.45,126455.45,126875.45,127295.45,127715.45,128135.45,128555.45,128975.45,129395.45,129815.45,136747.22,137167.22,137587.22,138007.22,138427.22,138847.22,139267.22,139687.22,140107.22,140527.22,140947.22,141367.22,148876.58,149296.58,149716.58,150136.58,150556.58,150976.58,151396.58,151816.58,152236.58,152656.58,153076.58,153496.58,161612.41,162032.41,162452.41,162872.41,163292.41,163712.41,164132.41,164552.41,164972.41,165392.41,165812.41,166232.41,174985.03",
        "0,0,0,0,0,0,0,0,0,0,0,252.00,0,0,0,0,0,0,0,0,0,0,0,516.60,0,0,0,0,0,0,0,0,0,0,0,794.43,0,0,0,0,0,0,0,0,0,0,0,1086.15,0,0,0,0,0,0,0,0,0,0,0,1392.46,0,0,0,0,0,0,0,0,0,0,0,1714.08,0,0,0,0,0,0,0,0,0,0,0,2051.79,0,0,0,0,0,0,0,0,0,0,0,2406.38,0,0,0,0,0,0,0,0,0,0,0,2778.69,0,0,0,0,0,0,0,0,0,0,0,3169.63,0,0,0,0,0,0,0,0,0,0,0,3580.11,0,0,0,0,0,0,0,0,0,0,0,4011.12,0,0,0,0,0,0,0,0,0,0,0,4463.67,0,0,0,0,0,0,0,0,0,0,0,4938.86,0,0,0,0,0,0,0,0,0,0,0,5437.80,0,0,0,0,0,0,0,0,0,0,0,5961.69,0,0,0,0,0,0,0,0,0,0,0,6511.77,0,0,0,0,0,0,0,0,0,0,0,7089.36,0,0,0,0,0,0,0,0,0,0,0,7695.83,0,0,0,0,0,0,0,0,0,0,0,8332.62")]
    public void CalculateYearlySavings_WithDifferentSettings_Returns(decimal targetAmount,
        int targetAdditionalMonthsFromNow,
        decimal interestRate,
        decimal expectedMonthlyPayment,
        short expectedMonthsNeeded,
        int expectedListLength,
        decimal amountExpectedTolerance,
        decimal interestsExpectedTolerance,
        string expectedAmountsCsv,
        string expectedInterestsCsv)
    {
        // Arrange
        var now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);
        var targetDate = now.AddMonths(targetAdditionalMonthsFromNow);
        decimal[] expectedAmounts = expectedAmountsCsv.ToDecimalArray(new CultureInfo("en-US"));
        decimal[] expectedInterests = expectedInterestsCsv.ToDecimalArray(new CultureInfo("en-US"));

        // Act
        var result = _instance.CalculateYearlySavings(targetAmount,
            targetDate,
            interestRate);

        // Assert
        result.ShouldNotBeNull();
        string amountCsv =
            string.Join(",", result.Amounts.Select(a => a.ToString(CultureInfo.GetCultureInfo("en-US"))));
        string interestCsv = string.Join(",",
            result.Interests.Select(i => i.ToString(CultureInfo.GetCultureInfo("en-US"))));
        result.MonthsNeeded.ShouldBe(expectedMonthsNeeded);
        result.MonthlyPayment.ShouldBe(expectedMonthlyPayment);
        // result.Amounts.Length.ShouldBe(expectedListLength);
        // result.Interests.Length.ShouldBe(expectedListLength);
        // result.Amounts.ShouldBe(expectedAmounts, amountExpectedTolerance);
        // result.Interests.ShouldBe(expectedInterests, interestsExpectedTolerance);
    }
}
