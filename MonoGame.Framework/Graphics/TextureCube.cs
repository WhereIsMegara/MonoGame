using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
 #if IPHONE || ANDROID
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using TextureMinFilter = OpenTK.Graphics.ES20.All;
 #endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class TextureCube : Texture
	{
		protected int size;
		
		PixelInternalFormat glInternalFormat;
		PixelFormat glFormat;
		PixelType glType;
		
		public TextureCube (GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format)
		{
			
			this.size = size;
			this.levelCount = 1;

			this.glTarget = TextureTarget.TextureCubeMap;

#if IPHONE || ANDROID
			GL.GenTextures(1, ref this.glTexture);
#else
			GL.GenTextures(1, out this.glTexture);
#endif
			GL.BindTexture (TextureTarget.TextureCubeMap, this.glTexture);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
			                mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
			                (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
			                (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
			                (int)TextureWrapMode.ClampToEdge);
			

			format.GetGLFormat (out glInternalFormat, out glFormat, out glType);
			
			for (int i=0; i<6; i++) {
				TextureTarget target = GetGLCubeFace((CubeMapFace)i);

				if (glFormat == (PixelFormat)All.CompressedTextureFormats) {
					throw new NotImplementedException();
				} else {
#if IPHONE || ANDROID
					GL.TexImage2D (target, 0, (int)glInternalFormat, size, size, 0, glFormat, glType, IntPtr.Zero);
#else
					GL.TexImage2D (target, 0, glInternalFormat, size, size, 0, glFormat, glType, IntPtr.Zero);
#endif
				}
			}
			
			if (mipMap)
			{
#if IPHONE || ANDROID
				GL.GenerateMipmap(TextureTarget.TextureCubeMap);
#else
				GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)All.True);
#endif
				
				int v = this.size;
				while (v > 1)
				{
					v /= 2;
					this.levelCount++;
				}
			}
			
		}
		
		public void SetData<T>(CubeMapFace face, int level, Rectangle? rect,
		                       T[] data, int startIndex, int elementCount) where T : struct
		{
            if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
			
			var xOffset = 0;
			var yOffset = 0;
			var width = Math.Max (1, this.size >> level);
			var height = Math.Max (1, this.size >> level);
			
			if (rect.HasValue)
			{
				xOffset = rect.Value.X;
				yOffset = rect.Value.Y;
				width = rect.Value.Width;
				height = rect.Value.Height;
			}
			
			GL.BindTexture (TextureTarget.TextureCubeMap, this.glTexture);
			
			TextureTarget target = GetGLCubeFace(face);
			if (glFormat == (PixelFormat)All.CompressedTextureFormats) {
				throw new NotImplementedException();
			} else {
				GL.TexSubImage2D(target, level, xOffset, yOffset, width, height, glFormat, glType, dataPtr);
			}
			
			dataHandle.Free ();
		}
		
		private TextureTarget GetGLCubeFace(CubeMapFace face) {
			switch (face) {
			case CubeMapFace.PositiveX: return TextureTarget.TextureCubeMapPositiveX;
			case CubeMapFace.NegativeX: return TextureTarget.TextureCubeMapNegativeX;
			case CubeMapFace.PositiveY: return TextureTarget.TextureCubeMapPositiveY;
			case CubeMapFace.NegativeY: return TextureTarget.TextureCubeMapNegativeY;
			case CubeMapFace.PositiveZ: return TextureTarget.TextureCubeMapPositiveZ;
			case CubeMapFace.NegativeZ: return TextureTarget.TextureCubeMapNegativeZ;
			}
			throw new ArgumentException();
		}

	}
}
