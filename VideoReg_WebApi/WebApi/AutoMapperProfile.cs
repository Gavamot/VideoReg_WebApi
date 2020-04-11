using AutoMapper;
using WebApi.Archive.ArchiveFiles;
using WebApi.Contract;
using WebApi.Controllers;
using WebApi.Dto;

namespace WebApi
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

            CreateMap<FileTrendsJson, FileTrendsDto>()
                .ForMember(dest => dest.Brigade, member => member.MapFrom(source => source.brigade))
                .ForMember(dest => dest.Pdt, member => member.MapFrom(source => source.pdt));
        }
    }
}