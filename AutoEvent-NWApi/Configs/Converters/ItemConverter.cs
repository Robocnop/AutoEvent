﻿// <copyright file="Log.cs" company="Redforce04#4091">
// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         AutoEvent
//    Project:          AutoEvent
//    FileName:         ItemConverter.cs
//    Author:           Redforce04#4091
//    Revision Date:    10/02/2023 10:25 PM
//    Created Date:     10/02/2023 10:25 PM
// -----------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AutoEvent.API;
using UnityEngine.Pool;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace AutoEvent.Configs.Converters;

public class ItemConverter : IYamlTypeConverter
{
    /// <inheritdoc cref="IYamlTypeConverter" />
    public bool Accepts(Type type) => type == typeof(ItemType) || type == typeof(Item) || type == typeof(Weapon);

    /// <inheritdoc cref="IYamlTypeConverter" />
    public object ReadYaml(IParser parser, Type type)
    {
        if (!parser.TryConsume<MappingStart>(out _))
            throw new InvalidDataException($"Cannot deserialize object of type {type.FullName}.");

        List<object> coordinates = ListPool<object>.Get();
        int i = 0;

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            if (i++ % 2 == 0)
            {
                parser.MoveNext();
                continue;
            }

            if (!parser.TryConsume(out Scalar scalar) || !float.TryParse(scalar.Value, NumberStyles.Float,
                    CultureInfo.GetCultureInfo("en-US"), out float coordinate))
            {
                ListPool<object>.Release(coordinates);
                throw new InvalidDataException($"Invalid float value.");
            }

            coordinates.Add(coordinate);
        }

        object vector = Activator.CreateInstance(type, coordinates.ToArray());

        ListPool<object>.Release(coordinates);

        return vector;
    }

    /// <inheritdoc cref="IYamlTypeConverter" />
    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        Dictionary<string, float> coordinates = DictionaryPool<string, float>.Get();

        if (value is Weapon weapon)
        {
            coordinates["item_type"] = weapon.ItemType;
            coordinates["attachments"] = weapon.Attachments;
        }
        else if (value is Item item)
        {
            coordinates["item_type"] = item;
        }
        

        emitter.Emit(new MappingStart());

        foreach (KeyValuePair<string, float> coordinate in coordinates)
        {
            emitter.Emit(new Scalar(coordinate.Key));
            emitter.Emit(new Scalar(coordinate.Value.ToString(CultureInfo.GetCultureInfo("en-US"))));
        }

        DictionaryPool<string, float>.Release(coordinates);
        emitter.Emit(new MappingEnd());
    }
}