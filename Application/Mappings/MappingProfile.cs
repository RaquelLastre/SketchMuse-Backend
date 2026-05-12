using AutoMapper;
using SketchMuse.Domain.DTOs;
using SketchMuse.Domain.Entities;

namespace SketchMuse.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Usuario, UsuarioResponseDTO>();
            CreateMap<Imagen, ImagenDTO>();
            CreateMap<Album, AlbumDTO>()
                .ForMember(dest => dest.NumImagenes, opt => opt.MapFrom(src => src.Imagenes.Count))
                .ForMember(dest => dest.PreviewImagenes, opt => opt.MapFrom(src =>
                    src.Imagenes.Take(3).Select(i => i.UrlSmall ?? i.Url ?? "").ToList()));
        }
    }
}
