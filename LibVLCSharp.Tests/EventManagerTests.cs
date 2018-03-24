﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VideoLAN.LibVLC;
using Media = VideoLAN.LibVLC.Media;

namespace LibVLCSharp.Tests
{
    [TestFixture]
    public class EventManagerTests
    {
        string RealMediaPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "sample.mp3");

        [Test]
        public void MetaChangedEventSubscribe()
        {
            var media = new Media(new Instance(), Path.GetTempFileName());
            var eventManager = media.EventManager;
            var eventHandlerCalled = false;
            const Media.MetadataType description = Media.MetadataType.Description;
            eventManager.MetaChanged += (sender, args) =>
            {
                Assert.AreEqual(description, args.MetadataType);
                eventHandlerCalled = true;
            };
            media.SetMeta(Media.MetadataType.Description, "test");
            Assert.True(eventHandlerCalled);
        }
        
        [Test]
        public void DurationChanged()
        {
            var media = new Media(new Instance(), RealMediaPath);
            var called = false;
            long duration = 0;

            media.EventManager.DurationChanged += (sender, args) =>
            {
                called = true;
                duration = args.Duration;
            };

            media.Parse();

            Assert.True(called);
            Assert.NotZero(duration);
        }

        [Test]
        public void FreedMedia()
        {
            var media = new Media(new Instance(), RealMediaPath, Media.FromType.FromPath);
            var eventCalled = false;
            media.EventManager.MediaFreed += (sender, args) =>
            {
                eventCalled = true;
            };

            media.Dispose();

            Assert.True(eventCalled);
        }

        [Test]
        public async Task StateChanged()
        {
            var media = new Media(new Instance(), RealMediaPath, Media.FromType.FromPath);
            var tcs = new TaskCompletionSource<bool>();
            var openingCalled = false;
            media.EventManager.StateChanged += (sender, args) =>
            {
                if (media.State == VLCState.Opening)
                {
                    openingCalled = true;
                    return;
                }
                Assert.AreEqual(VLCState.Playing, media.State);
                tcs.SetResult(true);
            };

            var mp = new MediaPlayer(media);
            mp.Play();
            await tcs.Task;
            Assert.True(tcs.Task.Result);
            Assert.True(openingCalled);
        }
    }
}