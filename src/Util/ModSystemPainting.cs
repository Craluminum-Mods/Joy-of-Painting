﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using ProtoBuf;

namespace jopainting
{
    class ModSystemPainting : ModSystem
    {
        public static ICoreAPI api;

        public Dictionary<string, TextureAtlasPosition> atlasPositions = new Dictionary<string, TextureAtlasPosition>();

        public override double ExecuteOrder()
        {
            return 0.3d;
        }

        public override void Start(ICoreAPI Api)
        {
            base.Start(Api);
            api = Api;
            api.Network.RegisterChannel("savepainting").RegisterMessageType<SavePaintingPacket>();
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
        }

        public TextureAtlasPosition GetAtlasPosition(PaintingBitmap painting, ICoreClientAPI capi, string picture)
        {
            if (atlasPositions.ContainsKey(picture)) return atlasPositions[picture];

            TextureAtlasPosition atlasPosition;
            int texSubId;

            capi.BlockTextureAtlas.InsertTexture(painting, out texSubId, out atlasPosition);

            atlasPositions.Add(picture, atlasPosition);

            return atlasPosition;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Network.GetChannel("savepainting").SetMessageHandler<SavePaintingPacket>(onSavePaintingPacket);
        }

        public void savePainting(IPlayer player, byte[] paintingR, byte[] paintingG, byte[] paintingB, int width, int height, string name)
        {
            //photographStack = new ItemStack(api.World.GetItem(new AssetLocation("kosphotography", "photograph")));

            /*ItemStack paintingStack = player.InventoryManager.ActiveHotbarSlot.Itemstack;

            paintingStack?.Attributes.SetInt("width", width);
            paintingStack?.Attributes.SetInt("height", height);
            paintingStack?.Attributes.SetString("paintingR", Encoding.GetEncoding(28591).GetString(paintingR));
            paintingStack?.Attributes.SetString("paintingG", Encoding.GetEncoding(28591).GetString(paintingG));
            paintingStack?.Attributes.SetString("paintingB", Encoding.GetEncoding(28591).GetString(paintingB));*/

            if (api is ICoreClientAPI capi)
            {
                capi.Network.GetChannel("savepainting").SendPacket(new SavePaintingPacket() { PaintingR = paintingR, PaintingG = paintingG, PaintingB = paintingB, Width = width, Height = height, Name = name });
                //capi.ShowChatMessage("Packet sent");
            }
            //player.InventoryManager.ActiveHotbarSlot.MarkDirty();
        }

        public void onSavePaintingPacket(IServerPlayer player, SavePaintingPacket packet)
        {
            //player.SendMessage(0, "Packet received", EnumChatType.Notification);
            //savePainting(player, packet.PaintingR, packet.PaintingG, packet.PaintingB, packet.Width, packet.Height);

            ItemStack paintingStack = player.InventoryManager.ActiveHotbarSlot.Itemstack;

            paintingStack?.Attributes.SetInt("width", packet.Width);
            paintingStack?.Attributes.SetInt("height", packet.Height);
            paintingStack?.Attributes.SetString("paintingR", Encoding.GetEncoding(28591).GetString(packet.PaintingR));
            paintingStack?.Attributes.SetString("paintingG", Encoding.GetEncoding(28591).GetString(packet.PaintingG));
            paintingStack?.Attributes.SetString("paintingB", Encoding.GetEncoding(28591).GetString(packet.PaintingB));
            paintingStack?.Attributes.SetString("paintingname", packet.Name);

            player.InventoryManager.ActiveHotbarSlot.MarkDirty();
        }

        public static Bitmap loadBmp(string bitmapName)
        {
            //return (Bitmap)Bitmap.FromFile(api.GetOrCreateDataPath("paintings") + "/moby.bmp");
            if (File.Exists(api.GetOrCreateDataPath("Paintings") + "/" + bitmapName + ".bmp"))
            {
                return (Bitmap)Bitmap.FromFile(api.GetOrCreateDataPath("Paintings") + "/" + bitmapName + ".bmp");
            }
            else
            {
                return new Bitmap(1, 1);
            }
        }
    }
    [ProtoContract]
    public class SavePaintingPacket
    {
        [ProtoMember(1)]
        public byte[] PaintingR;
        [ProtoMember(2)]
        public byte[] PaintingG;
        [ProtoMember(3)]
        public byte[] PaintingB;
        [ProtoMember(4)]
        public int Width;
        [ProtoMember(5)]
        public int Height;
        [ProtoMember(6)]
        public string Name;
    }
}
