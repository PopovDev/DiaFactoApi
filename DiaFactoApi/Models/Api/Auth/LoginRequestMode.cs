using System.Runtime.Serialization;

namespace DiaFactoApi.Models.Api.Auth;
public enum LoginRequestMode
{
    Web = 0,
    ExternalKey = 1,
}