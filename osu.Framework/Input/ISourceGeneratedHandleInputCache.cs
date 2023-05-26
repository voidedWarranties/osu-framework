// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Framework.Input
{
    public interface ISourceGeneratedHandleInputCache
    {
        protected internal bool RequestsPositionalInput { get; }
        protected internal bool RequestsNonPositionalInput { get; }
    }
}
