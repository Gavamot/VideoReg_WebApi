using AutoMapper;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.OnlineVideo;
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
            CreateMap<ImageTransformSettingsMV, ImageTransformSettings>();
            CreateMap<FileVideoMp4, FileVideoMp4Dto>()
                .ForMember(dest => dest.brig, member => member.MapFrom(source => source.brigade))
                .ForMember(dest => dest.name, member => member.MapFrom(source => source.fullArchiveName))
                .ForMember(dest => dest.num, member => member.MapFrom(source => source.cameraNumber))
                .ForMember(dest => dest.duration, member => member.MapFrom(source => source.durationSeconds));
        }
    }
}