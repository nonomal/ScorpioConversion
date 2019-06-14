﻿using System;
using System.Collections.Generic;
using System.Text;
using Scorpio;

public class FieldClass {
    private PackageParser mParser;
    public FieldClass(PackageParser parser) { mParser = parser; }
    public int Index;                   //字段索引
    public string Name;                 //字段名字
    public string Comment;              //字段注释
    public ScriptMap Attribute;         //字段属性,字段配置
    public string Default = "";         //字段默认值
    public string Type;                 //字段类型
    public bool Array = false;          //是否是数组
    public bool Valid = true;           //字段是否有效
    //是否是基本数据
    public bool IsBasic { get { return BasicUtil.HasType(Type); } }
    public BasicType BasicType { get { return BasicUtil.GetType(Type); } }
    public bool IsDateTime { get { return IsBasic && BasicType.Index == BasicEnum.DATETIME; } }

    public string AttributeString { get { return Attribute != null ? Attribute.ToJson(false) : "{}"; } }
    public bool IsEnum { get { return mParser != null && mParser.Enums.ContainsKey(Type); } }
    public int GetEnumValue(string value) {
        if (mParser == null)
            throw new Exception($"Parser 为空 EnumType : {Type}  Value : {value}");
        return mParser.GetEnumValue(Type, value);
    }
    public PackageClass CustomType {
        get {
            if (mParser == null)
                throw new Exception($"Parser 为空 CustomType : {Type}");
            return mParser.GetClasses(Type);
        }
    }
    public string GetLanguageType(Language language) {
        if (IsBasic) {
            return BasicType.GetLanguageType(language);
        } else if (language == Language.Go) {
            return IsEnum ? Type : $"*{Type}";
        } else {
            return Type;
        }
    }

}
public class PackageClass : IPackage {
    public List<FieldClass> Fields = new List<FieldClass>();
}