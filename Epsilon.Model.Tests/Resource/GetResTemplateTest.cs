using System;
using System.Collections.Generic;
using Epsilon.Model.Resource;
using Xunit;

namespace Epsilon.Model.Tests.Resource
{
    public class GetResTemplateTest
    {
        [Fact]
        public void AnonymousObjectTemplate()
        {
            var service = new ResourceService(new FakeResModel("test {{Name}} test"));
            var r = service.GetRes("Res.Test", 12, new {Name = "test"});
            Assert.Equal("test test test", r);
        }
        

        private class FakeResModel : ResourceModel
        {
            private readonly string _fakeValue;

            public FakeResModel(string fakeValue) : base(new FakeResConfig())
            {
                _fakeValue = fakeValue;
            }

            internal override Value GetResValue(string resName, int cultureId)
            {
                return new Value
                {
                    ResName = resName,
                    ResValue = _fakeValue,
                    DefaultValue = _fakeValue
                };
            }
        }

        private class FakeResConfig : IConfig
        {
            public string ConnectionString  => string.Empty;
        }
    }
}
