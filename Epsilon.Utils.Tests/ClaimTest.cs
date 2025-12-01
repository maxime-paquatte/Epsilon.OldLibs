using System;
using System.Linq;
using Xunit;

namespace Epsilon.Utils.Tests
{
    
    
    public class ClaimTest
    {
        readonly string[] _userClaims = {"User.AAA", "User.BBB", "User.CCC"};
        private readonly string[] _admin;

        public ClaimTest()
        {
            _admin = _userClaims.Union(new [] {"User.SysAdmin"}).ToArray();
        }
        
        
        [Theory]
        [InlineData("%.AAA")]
        [InlineData("%.AAA|%.BBB")]
        [InlineData("%.AAA&%.BBB")]
        [InlineData("%.AAA|%.DDD")]
        [InlineData("%.AAA|(%.BBB|%.DDD)")]
        [InlineData("%.AAA|(%.BBB&%.DDD)")]
        public void ClaimOk(string claim)
        {
            Assert.True(Claims.Validate(_userClaims, claim), $"Claim '{claim}' should be valid for user");
        }

        [Theory]
        [InlineData("%.AAA&%.DDD")]
        [InlineData("%.DDD&(%.AAA|%.BBB)")]
        public void ClaimKo(string claim)
        {
            Assert.False(Claims.Validate(_userClaims, claim), $"Claim '{claim}' should be invalid for user");
            Assert.True(Claims.Validate(_admin, claim), $"Claim '{claim}' should be valid for admin");
        }
       
        [Fact]
        public void ClaimEmpty()
        {
            Assert.False(Claims.Validate(_userClaims, string.Empty), $"Empty claim should be invalid for user");
            Assert.True(Claims.Validate(_admin, string.Empty), $"Empty claim should be valid for admin");
        }
        
        [Fact]
        public void NegativeClaims()
        {
            Assert.False(Claims.Validate(_userClaims,"!User.AAA"), $"Claim '!User.AAA' should be invalid for user");
            Assert.True(Claims.Validate(_admin, "!User.AAA"), $"Claim '!User.AAA' should be valid for admin");
        }
    }
}
