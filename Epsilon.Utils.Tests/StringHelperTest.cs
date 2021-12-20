using System;
using Xunit;

namespace Epsilon.Utils.Tests
{
    public class StringHelperTest
    {
        [Theory]
        [InlineData("someone@somewhere.com")]
        [InlineData("someone@somewhere.co.uk")]
        [InlineData("someone+tag@somewhere.net")]
        [InlineData("futureTLD@somewhere.fooo")]
        public void MailOk(string email)
        {
            Assert.True(StringHelper.IsValidEmail(email));
        }

        [Theory]
        [InlineData("fdsa")]
        [InlineData("fdsa@")]
        [InlineData("fdsa@fdsa")]
        [InlineData("fdsa@fdsa.")]
        public void MailKo(string email)
        {
            Assert.False(StringHelper.IsValidEmail(email));
        }

    }
}
