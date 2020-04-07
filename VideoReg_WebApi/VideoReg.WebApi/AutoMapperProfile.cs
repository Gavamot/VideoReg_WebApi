using AutoMapper;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.Contract;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Dto;
using VideoReg.WebApi.Controllers;
using VideoReg.WebApi.Dto;

namespace VideoReg.WebApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateCameraProfile();
        }

        private void CreateCameraProfile()
        {
            CreateMap<ImageTransformSettingsDto, ImageSettings>();
            CreateMap<ImageSettings, ImageTransformSettingsDto>();
            CreateMap<FileVideoMp4, FileVideoMp4Dto>()
                .ForMember(dest => dest.Brigade, member => member.MapFrom(source => source.brigade))
                .ForMember(dest => dest.Pdt, member => member.MapFrom(source => source.pdt))
                .ForMember(dest => dest.Camera, member => member.MapFrom(source => source.cameraNumber))
                .ForMember(dest => dest.Duration, member => member.MapFrom(source => source.durationSeconds));
        }
    }
}