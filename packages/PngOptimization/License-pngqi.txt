
pngnqi.exe and pngquanti.exe are based on 'Improved PNGNQ' and 'Improved PNGQuant' by Kornel Lesinski, which were derived from the original pngnq and pngquant tools.

The respective home pages of these tools are:

http://pornel.net/pngnq
http://pornel.net/pngquant

BATCH FILES

I have also included these batch files to make working with these tools easier in the absense of a GUI.  I hereby release them into the public domain; do with them what you want.  No warranty.
Process - Palette reduce 256 NeuQuant.bat
Process - Palette reduce 64 NeuQuant.bat
Process - Palette reduce 256 Median.bat
Process - Palette reduce 64 Median.bat

You should be able to drag and drop your 24-bit PNGs onto these batch files as an easy way to process them without messing around with the command line.  Dithered and non-dithered optimised copies of the source PNGs will be created in the same directory as the originals.  The batch files must remain in the same directory as pngnqi.exe and pngquanti.exe.

- Thomas Rutter

COPYRIGHT AND LICENSES

Full copyright notices and licenses of components can be found in the respective source code directories.

Improved PNGNQ is
- Copyright (C) 1989, 1991 by Jef Poskanzer.
- Copyright (C) 1997, 2000, 2002 by Greg Roelofs; based on an idea by Stefan Schneider.
- Copyright (C) 2004-2007 by Stuart Coyle
** Permission to use, copy, modify, and distribute this software and its
** documentation for any purpose and without fee is hereby granted, provided
** that the above copyright notice appear in all copies and that both that
** copyright notice and this permission notice appear in supporting
** documentation.  This software is provided "as is" without express or
** implied warranty.

Improved PNGNQ includes an implementation of the Neuquant algorithm that is
- Copyright (c) 1994 Anthony Dekker
* Any party obtaining a copy of these files from the author, directly or
* indirectly, is granted, free of charge, a full and unrestricted irrevocable,
* world-wide, paid up, royalty-free, nonexclusive right and license to deal
* in this software and documentation files (the "Software"), including without
* limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons who receive
* copies from any such party to do so, with the only requirement being
* that this copyright notice remain intact.

Improved PNGQuant is
- Copyright (C) 1989, 1991 by Jef Poskanzer
- Copyright (C) 1997, 2000, 2002 by Greg Roelofs; based on an idea by Stefan Schneider.
- Copyright (C) 2009 by Kornel Lesinski.
** Permission to use, copy, modify, and distribute this software and its
** documentation for any purpose and without fee is hereby granted, provided
** that the above copyright notice appear in all copies and that both that
** copyright notice and this permission notice appear in supporting
** documentation.  This software is provided "as is" without express or
** implied warranty.

Both tools include portions of libpng, which is
* Copyright (c) 1998-2009 Glenn Randers-Pehrson
* (Version 0.96 Copyright (c) 1996, 1997 Andreas Dilger)
* (Version 0.88 Copyright (c) 1995, 1996 Guy Eric Schalnat, Group 42, Inc.)
- Full license is supplied in png.h file, but here is an excerpt:
* The PNG Reference Library is supplied "AS IS".  The Contributing Authors
* and Group 42, Inc. disclaim all warranties, expressed or implied,
* including, without limitation, the warranties of merchantability and of
* fitness for any purpose.  The Contributing Authors and Group 42, Inc.
* assume no liability for direct, indirect, incidental, special, exemplary,
* or consequential damages, which may result from the use of the PNG
* Reference Library, even if advised of the possibility of such damage.
*
* Permission is hereby granted to use, copy, modify, and distribute this
* source code, or portions hereof, for any purpose, without fee, subject
* to the following restrictions:
*
* 1. The origin of this source code must not be misrepresented.
*
* 2. Altered versions must be plainly marked as such and
* must not be misrepresented as being the original source.
*
* 3. This Copyright notice may not be removed or altered from
*    any source or altered source distribution.
*
* The Contributing Authors and Group 42, Inc. specifically permit, without
* fee, and encourage the use of this source code as a component to
* supporting the PNG file format in commercial products.  If you use this
* source code in a product, acknowledgment is not required but would be
* appreciated.

Both tools also include portions of zlib, which is
- Copyright (C) 1995-2005 Jean-loup Gailly and Mark Adler
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* 
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.