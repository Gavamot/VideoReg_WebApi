using AutoMapper;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Dto;
using VideoReg.WebApi.Controllers;

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
            CreateMap<ImageTransformSettingsMV, ImageSettings>();
            CreateMap<ImageSettings, ImageTransformSettingsMV>();
            CreateMap<FileVideoMp4, FileVideoMp4Dto>()
                .ForMember(dest => dest.Brig, member => member.MapFrom(source => source.brigade))
                .ForMember(dest => dest.Name, member => member.MapFrom(source => source.fullArchiveName))
                .ForMember(dest => dest.Num, member => member.MapFrom(source => source.cameraNumber))
                .ForMember(dest => dest.Duration, member => member.MapFrom(source => source.durationSeconds));
        }
    }
}