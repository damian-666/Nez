﻿using System;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and Entity.debugRender.
	/// A simple Renderer could just start the Graphics.defaultGraphics spriteBatch or it could create its own local Graphics instance
	/// if it needs it for some kind of custom rendering.
	/// 
	/// Note that it is a best practice to ensure all Renderers that render to a RenderTexture have lower renderOrders to avoid issues
	/// with clearing the back buffer (http://gamedev.stackexchange.com/questions/90396/monogame-setrendertarget-is-wiping-the-backbuffer).
	/// Giving them a negative renderOrder is a good strategy to deal with this.
	/// </summary>
	public abstract class Renderer
	{
		/// <summary>
		/// Comparison used to sort renderers
		/// </summary>
		static public Comparison<Renderer> compareRenderOrder = ( a, b ) => { return Math.Sign( a.renderOrder - b.renderOrder ); };

		/// <summary>
		/// BlendState used by the SpriteBatch
		/// </summary>
		public BlendState blendState = BlendState.AlphaBlend;

		/// <summary>
		/// SamplerState used by the SpriteBatch
		/// </summary>
		public SamplerState samplerState = SamplerState.PointClamp;

		/// <summary>
		/// Effect used by the SpriteBatch
		/// </summary>
		public Effect effect;

		/// <summary>
		/// the Camera this renderer uses for rendering (really its transformMatrix and bounds for culling). If it is null, the scenes Camera
		/// will be used.
		/// </summary>
		public Camera camera;

		/// <summary>
		/// specifies the order in which the Renderers will be called by the scene
		/// </summary>
		public readonly int renderOrder = 0;

		/// <summary>
		/// if renderTexture is not null this renderer will render into the RenderTexture instead of to the screen
		/// </summary>
		public RenderTexture renderTexture;

		/// <summary>
		/// if renderTexture is not null this Color will be used to clear the screen
		/// </summary>
		public Color renderTextureClearColor = Color.Transparent;


		public Renderer( Camera camera, int renderOrder )
		{
			this.camera = camera;
			this.renderOrder = renderOrder;
		}


		/// <summary>
		/// if a RenderTexture is used this will set it up. The SpriteBatch is also started.
		/// </summary>
		/// <param name="cam">Cam.</param>
		protected virtual void beginRender( Camera cam )
		{
			// if we have a renderTexture render into it
			if( renderTexture != null )
			{
				Graphics.defaultGraphics.graphicsDevice.SetRenderTarget( renderTexture );
				Graphics.defaultGraphics.graphicsDevice.Clear( renderTextureClearColor );
			}

			// MonoGame resets the Viewport to the RT size without asking so we have to let the Camera know to update itself
			cam.forceMatrixUpdate();

			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.None, RasterizerState.CullNone, effect, cam.transformMatrix );
		}


		abstract public void render( Scene scene );


		/// <summary>
		/// ends the SpriteBatch and clears the RenderTarget if it had a RenderTexture
		/// </summary>
		protected virtual void endRender()
		{
			Graphics.defaultGraphics.spriteBatch.End();

			// clear the RenderTarget so that we render to the screen if we were using a RenderTexture
			if( renderTexture != null )
				Graphics.defaultGraphics.graphicsDevice.SetRenderTarget( null );
		}


		/// <summary>
		/// default debugRender method just loops through all entities and calls entity.debugRender
		/// </summary>
		/// <param name="scene">Scene.</param>
		public virtual void debugRender( Scene scene )
		{
			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, scene.camera.transformMatrix );

			foreach( var entity in scene.entities )
			{
				if( entity.enabled )
					entity.debugRender( Graphics.defaultGraphics );
			}

			Graphics.defaultGraphics.spriteBatch.End();
		}


		/// <summary>
		/// called when a scene is ended. use this for cleanup.
		/// </summary>
		public virtual void unload()
		{
			if( renderTexture != null )
			{
				renderTexture.unload();
				renderTexture = null;
			}
		}
	
	}
}

