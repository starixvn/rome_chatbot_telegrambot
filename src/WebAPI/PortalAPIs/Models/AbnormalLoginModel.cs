namespace PortalAPIs.Models;

public record AbnormalLoginModel(
    long ID,
    string LoginName,
    DateTime TransTime,
    string UserAgent,
    string IP,
    string OS,
    string DeviceTypeName,
    string Browser);