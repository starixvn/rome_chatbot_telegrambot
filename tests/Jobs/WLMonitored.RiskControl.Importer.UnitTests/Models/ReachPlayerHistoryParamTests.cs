using System.Text;
using WLMonitored.RiskControl.Importer.Models;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Criteria;

public class ReachPlayerHistoryParamTests
{
    [Fact]
    public void AppendCustXml_Always_ReturnsCorrectXml()
    {
        // Arrange
        var sb = new StringBuilder();
        var param = new ReachPlayerHistoryParam(
            23003140,
            100,
            0,
            1,
            200,
            300,
            2
        );

        // Act
        param.AppendCustXml(sb);

        // Assert
        Assert.Equal("<Cust CustId=\"23003140\" AccumWL=\"100\" SportType=\"0\" SettingType=\"1\" WinningThreshold=\"200\" LosingThreshold=\"300\" ReachType=\"2\"/>", sb.ToString());
    }
}