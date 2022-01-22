﻿using System.Text;
namespace Scorpio.Conversion {
    [AutoGenerator("c#")]
    public class GeneratorCSharp : IGenerator {
        public const string Head = @"//本文件为自动生成，请不要手动修改
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Scorpio.Conversion;
";
        static string GetLanguageType(ClassField field) {
            if (field.IsBasic) {
                return field.BasicType.Index switch {
                    BasicEnum.BOOL => "bool",
                    BasicEnum.INT8 => "sbyte",
                    BasicEnum.UINT8 => "byte",
                    BasicEnum.INT16 => "short",
                    BasicEnum.UINT16 => "ushort",
                    BasicEnum.INT32 => "int",
                    BasicEnum.UINT32 => "uint",
                    BasicEnum.INT64 => "long",
                    BasicEnum.UINT64 => "ulong",
                    BasicEnum.FLOAT => "float",
                    BasicEnum.DOUBLE => "double",
                    BasicEnum.STRING => "string",
                    BasicEnum.DATETIME => "DateTime",
                    BasicEnum.BYTES => "byte[]",
                    _ => throw new System.Exception("未知的基础类型"),
                };
            } else {
                return field.Type;
            }
        }
        public override string GenerateTableClass(string packageName, string tableClassName, string dataClassName, string fileMD5, PackageClass packageClass) {
            var keyType = GetLanguageType(packageClass.Fields[0]);
            return $@"{Head}
namespace {packageName} {{
    public partial class {tableClassName} : ITable {{
        const string FILE_MD5_CODE = ""{fileMD5}"";
        private int m_count = 0;
        private Dictionary<{keyType}, {dataClassName}> m_dataArray = new Dictionary<{keyType}, {dataClassName}>();
        public {tableClassName} Initialize(string fileName, IReader reader) {{
            var row = reader.ReadInt32();
            var layoutMD5 = reader.ReadString();
            if (layoutMD5 != FILE_MD5_CODE) {{
                throw new Exception(""File schemas do not match [{tableClassName}] : "" + fileName);
            }}
            ConversionUtil.ReadHead(reader);
            for (var i = 0; i < row; ++i) {{
                var pData = new {dataClassName}(fileName, reader);
                if (m_dataArray.TryGetValue(pData.ID, out var value))
                    value.Set(pData);
                else
                    m_dataArray[pData.ID] = pData;
            }}
            m_count = m_dataArray.Count;
            return this;
        }}
        public {dataClassName} GetValue({keyType} ID) {{
            if (m_dataArray.TryGetValue(ID, out var value))
                return value;
            throw new Exception($""{tableClassName} not found data : {{ID}}"");
        }}
        public bool Contains({keyType} ID) {{
            return m_dataArray.ContainsKey(ID);
        }}
        public Dictionary<{keyType}, {dataClassName}> Datas() {{
            return m_dataArray;
        }}
        public IData GetValueObject(object ID) {{
            return GetValue(({keyType})ID);
        }}
        public bool ContainsObject(object ID) {{
            return Contains(({keyType})ID);
        }}
        public IDictionary GetDatas() {{
            return Datas();
        }}
        public int Count() {{
            return m_count;
        }}
    }}
}}";
        }
        public override string GenerateDataClass(string packageName, string className, PackageClass packageClass, bool createID) {
            return $@"{Head}
namespace {packageName} {{
public partial class {className} : IData {{
    {AllFields(packageClass, createID)}
    {FunctionConstructor(packageClass, className)}
    {FunctionGetData(packageClass)}
    {FunctionSet(packageClass, className)}
    {FunctionToString(packageClass)}
}}
}}
";
        }
        string AllFields(PackageClass packageClass, bool createID) {
            var builder = new StringBuilder();
            var first = true;
            foreach (var field in packageClass.Fields) {
                var languageType = GetLanguageType(field);
                if (field.IsArray) { languageType = $"ReadOnlyCollection<{languageType}>"; }
                if (first) {
                    first = false;
                    if (createID && field.Name != "ID") {
                        builder.Append($@"
    public {languageType} ID => {field.Name};");
                    }
                }
                builder.Append($@"
    /* <summary> {field.Comment}  默认值({field.Default}) </summary> */
    public {languageType} {field.Name} {{ get; private set; }}");

            }
            return builder.ToString();
        }
        string FunctionConstructor(PackageClass packageClass, string dataClassName) {
            var builder = new StringBuilder();
            builder.Append($@"
    public {dataClassName}(string fileName, IReader reader) {{");
            foreach (var field in packageClass.Fields) {
                var languageType = GetLanguageType(field);
                string fieldRead;
                if (field.Attribute != null && field.Attribute.GetValue("Language").IsTrue) {
                    fieldRead = $@"reader.ReadL10N(fileName + "".{field.Name}."" + this.ID)";
                } else if (field.IsBasic) {
                    fieldRead = $"reader.Read{field.BasicType.Name}()";
                } else if (field.IsEnum) {
                    fieldRead = $"({languageType})reader.ReadInt32()";
                } else {
                    fieldRead = $"new {languageType}(fileName, reader)";
                }
                if (field.IsArray) {
                    builder.Append($@"
        {{
            var list = new List<{languageType}>();
            var number = reader.ReadInt32();
            for (int i = 0; i < number; ++i) {{ list.Add({fieldRead}); }}
            this.{field.Name} = list.AsReadOnly();
        }}");
                } else {
                    builder.Append($@"
        this.{field.Name} = {fieldRead};");
                }
            }
            builder.Append(@"
    }");
            return builder.ToString();
        }
        string FunctionGetData(PackageClass packageClass) {
            var builder = new StringBuilder();
            builder.Append(@"
    public object GetData(string key) {");
            foreach (var field in packageClass.Fields) {
                builder.Append($@"
        if (""{field.Name}"".Equals(key)) return {field.Name};");
            }
            builder.Append(@"
        return null;
    }");
            return builder.ToString();
        }
        string FunctionSet(PackageClass packageClass, string dataClassName) {
            var builder = new StringBuilder();
            builder.Append($@"
    public void Set({dataClassName} value) {{");
            foreach (var field in packageClass.Fields) {
                builder.Append($@"
        this.{field.Name} = value.{field.Name};");
            }
            builder.Append(@"
    }");
            return builder.ToString();
        }
        string FunctionToString(PackageClass packageClass) {
            var builder = new StringBuilder();
            builder.Append(@"
    public override string ToString() {
        return $""");
            foreach (var field in packageClass.Fields) {
                builder.AppendFormat("{0}:{1}, ", field.Name, $"{{{field.Name}}}");
            }
            builder.Append(@""";
    }");
            return builder.ToString();
        }
        public override string GenerateEnumClass(string packageName, string className, PackageEnum packageEnum) {
            var builder = new StringBuilder();
            builder.Append($@"//本文件为自动生成，请不要手动修改
namespace {packageName} {{
    public enum {className} {{");
            foreach (var info in packageEnum.Fields) {
                builder.Append($@"
        {info.Name} = {info.Index},");
            }
            builder.Append(@"
    }
}");
            return builder.ToString();
        }
    }
}
