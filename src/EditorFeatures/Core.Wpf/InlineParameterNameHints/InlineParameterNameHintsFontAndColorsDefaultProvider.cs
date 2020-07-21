// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.CodeAnalysis.Editor.InlineParameterNameHints
{
    [Guid("FE84602A-43A8-4D48-9BAD-E2870B875FA0")]
    internal class InlineParameterNameHintsFontAndColorsDefaultProvider : IVsFontAndColorDefaultsProvider
    {
        private InlineParameterNameHintsFontandColors _hintsFontandColors;
        public InlineParameterNameHintsFontAndColorsDefaultProvider(IVsFontAndColorStorage storage)
        {
            _hintsFontandColors = new InlineParameterNameHintsFontandColors(storage);
        }

        public int GetObject(ref Guid rguidCategory, out object ppObj)
        {
            throw new NotImplementedException();
        }
    }

    internal class InlineParameterNameHintsFontandColors : IVsFontAndColorDefaults, IVsFontAndColorEvents
    {
        public InlineParameterNameHintsFontandColors(IVsFontAndColorStorage storage)
        {

        }

        public int GetFlags(out uint dwFlags)
        {
            throw new NotImplementedException();
        }

        public int GetPriority(out ushort pPriority)
        {
            throw new NotImplementedException();
        }

        public int GetCategoryName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public int GetBaseCategory(out Guid pguidBase)
        {
            throw new NotImplementedException();
        }

        public int GetFont(FontInfo[] pInfo)
        {
            throw new NotImplementedException();
        }

        public int GetItemCount(out int pcItems)
        {
            throw new NotImplementedException();
        }

        public int GetItem(int iItem, AllColorableItemInfo[] pInfo)
        {
            throw new NotImplementedException();
        }

        public int GetItemByName(string szItem, AllColorableItemInfo[] pInfo)
        {
            throw new NotImplementedException();
        }

        public int OnFontChanged(ref Guid rguidCategory, FontInfo[] pInfo, LOGFONTW[] pLOGFONT, uint HFONT)
        {
            throw new NotImplementedException();
        }

        public int OnItemChanged(ref Guid rguidCategory, string szItem, int iItem, ColorableItemInfo[] pInfo, uint crLiteralForeground, uint crLiteralBackground)
        {
            throw new NotImplementedException();
        }

        public int OnReset(ref Guid rguidCategory)
        {
            throw new NotImplementedException();
        }

        public int OnResetToBaseCategory(ref Guid rguidCategory)
        {
            throw new NotImplementedException();
        }

        public int OnApply()
        {
            throw new NotImplementedException();
        }
    }
}
