using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LKDS_Type
{
    public static class EnumHelper
    {
        public static string GetNameOfEnum(this Enum enumVal)
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            try
            {
                var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                return (attributes.Length > 0) ? ((DescriptionAttribute)attributes[0]).Description : "";
            }
            catch
            {
                return Convert.ToInt16(enumVal).ToString("X4");
            }

        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union16
        {
            [FieldOffset(0)]
            public Int16 Value;
            [NonSerialized]
            [FieldOffset(0)]
            public UInt16 UValue;
            [FieldOffset(0)]
            public byte Byte0;
            [FieldOffset(1)]
            public byte Byte1;
            public bool isBitSet(byte index)
            {
                if (index >= 15)
                    return false;
                return (Value & (1L << index)) != 0;
            }
            [System.Xml.Serialization.XmlIgnore]
            public byte[] ToArray => new byte[]
            {
            Byte0,
            Byte1
            };
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union32
        {
            [FieldOffset(0)]
            public Int32 Value;
            [NonSerialized]
            [FieldOffset(0)]
            public UInt32 UValue;
            [FieldOffset(0)]
            public byte Byte0;
            [FieldOffset(1)]
            public byte Byte1;
            [FieldOffset(2)]
            public byte Byte2;
            [FieldOffset(3)]
            public byte Byte3;
            public bool isBitSet(byte index)
            {
                if (index >= 31)
                    return false;
                return (Value & (1L << index)) != 0;
            }
            [System.Xml.Serialization.XmlIgnore]
            public byte[] ToArray => new byte[]
            {
            Byte0,
            Byte1,
            Byte2,
            Byte3
            };
        }

        public enum CAN_Devices
        {
            [Description("ЛБ/Концентратор")]
            LB = 0,
            [Description("USB - VOICE converter")]
            USB = 2,
            [Description("VRP - VIDEO converter")]
            VRP = 3,
            [Description("Переговорное устройство v7(ПУv7)")]
            PU = 4,
            [Description("Переговорное устройство этажное v7(ЭПУv7)")]
            EPU = 5,
            [Description("Удлинитель WiFi v7")]
            wifi = 6,
            [Description("Адаптер входов v7(АСК-16)")]
            ASK = 7,
            [Description("Адаптер ТУ v7(АТУ-8*2)")]
            ATU = 8,
            [Description("Адаптер ПУ v7(АПУ-1)")]
            APU = 9,
            [Description("Адаптер Последовательного Интерфейса(АПИ-1)")]
            API = 10,
            [Description("Портал Контроллер Доступа (ПКД2*2)")]
            PKD22 = 11,
            [Description("Портал Контроллер Доступа (ПКД2*16)")]
            PKD216 = 12,
            [Description("Переговорное устр. Аккум. платформы (ПУ АП)")]
            PUAP = 13,
            [Description("Адаптер релейных выходов (АРВ-8*6)")]
            ARV = 14,
            [Description("Адаптер Лампа Индикаторная (АЛИ-1)")]
            ALI = 15,
            [Description("Адаптер Токовых Сигналов (АТС-4*4)")]
            ATS = 16,
            [Description("Адаптер ModBUS (АМБ-1)")]
            AMB = 17,
            [Description("Адаптер Звукового Оповещения (АЗО-1)")]
            AZO = 18,
            [Description("Адаптер Переговорного Устройства 2 (АПУ-2)")]
            APU2 = 19,
            [Description("Переговорное устройство посадоч. площ.(ПУ ПП)")]
            PUPP = 20,
            [Description("Выносной Модуль Управления (ВМУ)")]
            VMU = 21,
            [Description("Адаптер шлейфов (АШЛ-6*4)")]
            ASHL = 22,
            [Description("Портал Контроллер Доступа (ПКД1*2)")]
            PKD12 = 24,
            [Description("ПКД2*2-режим Команд")]
            PKD22CR = 25,
            [Description("ПКД2*16-режим Команд")]
            PKD216CR = 26,
            [Description("ПКД2*1-режим Команд")]
            PKD21CR = 27,
            [Description("Тип не определен")]
            notype = 255,
        }
    }
}
