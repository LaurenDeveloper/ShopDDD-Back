﻿using System;
using System.Collections.Generic;
using SharedKernel.Domain.Entities;

namespace Sinmark.Domain
{
    // TODO ALVARO: Dudas respecto a la otra entidad User en Authentication.Domain

    internal class User : EntityAuditable<Guid>
    {
        public string Name { get; }

        public IEnumerable<WishList> WishLists { get; }

        /// <summary>
        /// Example: es, fr, pt
        /// </summary>
        public string Language { get; }
    }
}
