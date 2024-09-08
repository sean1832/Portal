﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PTexture
    {
        public string Path { get; }
        public PTextureType Type { get; }
        public PTexture(string path, PTextureType type)
        {
            Path = path;
            Type = type;
        }
    }
}
