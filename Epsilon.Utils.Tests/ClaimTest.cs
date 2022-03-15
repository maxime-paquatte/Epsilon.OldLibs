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
            Assert.True(Claims.Validate(_userClaims, claim));
        }

        [Theory]
        [InlineData("%.AAA&%.DDD")]
        [InlineData("%.DDD&(%.AAA|%.BBB)")]
        public void ClaimKo(string claim)
        {
            Assert.False(Claims.Validate(_userClaims, claim));
            
            Assert.True(Claims.Validate(_admin, claim));
        }
       

    }
}
