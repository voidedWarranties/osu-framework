// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Framework
{
    /// <summary>
    /// Various configuration properties for a <see cref="Host"/>.
    /// </summary>
    public class HostOptions
    {
        /// <summary>
        /// Name of the game or application.
        /// </summary>
        /// <remarks>
        /// This property may be null. Host types like <see cref="Platform.HeadlessGameHost"/> fallback to a custom GUID string when it occurs.
        /// </remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Whether to bind the IPC port.
        /// </summary>
        public bool BindIPC { get; set; }

        /// <summary>
        /// Whether it runs in real time.
        /// </summary>
        /// <remarks>
        /// This is particularly relevant for a <see cref="Platform.HeadlessGameHost"/>.
        /// Any other host, such as <see cref="Platform.DesktopGameHost"/>, is assumed to run in real time.
        /// </remarks>
        public bool Realtime { get; set; } = true;

        /// <summary>
        /// Whether this is a portable installation.
        /// </summary>
        public bool PortableInstallation { get; set; }

        /// <summary>
        /// Whether to bypass the compositor. Defaults to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// On Linux, the compositor re-buffers the application to apply various window effects,
        /// increasing latency in the process. Thus it is a good idea for games to bypass it,
        /// though normal applications would generally benefit from letting the window effects untouched. <br/>
        /// If the SDL_VIDEO_X11_NET_WM_BYPASS_COMPOSITOR environment variable is set, this property will have no effect.
        /// </remarks>
        public bool BypassCompositor { get; set; } = true;
    }
}
