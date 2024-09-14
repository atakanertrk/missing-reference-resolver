using AutoMapper;
using System;

namespace DemoClassLibrary
{
    public class CustomMapper
    {
        public void Configure()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceDTO, TargetDTO>();
            });
        }
    }
}
