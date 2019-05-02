﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace P.Runtime
{
    [Serializable]
    public abstract class PrtValue
    {
        public static PrtEventValue @null = new PrtEventValue(new PrtEvent("null", new PrtNullType(), PrtEvent.DefaultMaxInstances, false));
        public static PrtEventValue halt = new PrtEventValue(new PrtEvent("halt", new PrtAnyType(), PrtEvent.DefaultMaxInstances, false));

        public abstract PrtValue Clone();


        public static PrtValue PrtMkDefaultValue(PrtType type)
        {
            if (type is PrtAnyType || type is PrtNullType || type is PrtEventType || type is PrtMachineType || type is PrtInterfaceType)
            {
                return @null;
            }
            else if (type is PrtIntType)
            {
                return new PrtIntValue();
            }
            else if (type is PrtFloatType)
            {
                return new PrtFloatValue();
            }
            else if (type is PrtEnumType)
            {
                PrtEnumType enumType = type as PrtEnumType;
                return new PrtEnumValue(enumType.DefaultConstant, 0);
            }
            else if (type is PrtBoolType)
            {
                return new PrtBoolValue();
            }
            else if (type is PrtMapType)
            {
                return new PrtMapValue();
            }
            else if (type is PrtSeqType)
            {
                return new PrtSeqValue();
            }
            else if (type is PrtNamedTupleType)
            {
                return new PrtNamedTupleValue(type as PrtNamedTupleType);
            }
            else if (type is PrtTupleType)
            {
                return new PrtTupleValue(type as PrtTupleType);
            }
            else
            {
                throw new PrtInternalException("Invalid type in PrtMkDefaultType");
            }
        }

        public override string ToString()
        {
            throw new NotImplementedException("ToString method is not overridden in the derived class");
        }

        public virtual int Size()
        {
            throw new NotImplementedException("Size method is not overridden in the derived class");
        }

        //public abstract bool Equals(PrtValue val);

        public static bool PrtInhabitsType(PrtValue value, PrtType type)
        {
            if (type is PrtAnyType)
            {
                return true;
            }
            else if (value.Equals(@null))
            {
                return (type is PrtNullType || type is PrtEventType || type is PrtMachineType);
            }
            else if (type is PrtEnumType)
            {
                PrtEnumType enumType = type as PrtEnumType;
                PrtIntValue intValue = value as PrtIntValue;
                if (intValue == null) return false;
                return enumType.enumConstants.ContainsKey(intValue.nt);
            }
            else if (type is PrtIntType)
            {
                return value is PrtIntValue;
            }
            else if (type is PrtFloatType)
            {
                return value is PrtFloatValue;
            }
            else if (type is PrtBoolType)
            {
                return value is PrtBoolValue;
            }
            else if (type is PrtEventType)
            {
                return value is PrtEventValue;
            }
            else if (type is PrtMachineType)
            {
                return (value is PrtMachineValue || value is PrtInterfaceValue);
            }
            else if (type is PrtInterfaceType)
            {
                var interValue = value as PrtInterfaceValue;
                if(interValue == null)
                {
                    return false;
                }
                else
                {
                    if(interValue.permissions == null)
                    {
                        return (type as PrtInterfaceType).permissions == null;
                    }
                    else
                    {
                        if((type as PrtInterfaceType).permissions == null)
                        {
                            return false;
                        }
                    }

                    if(interValue.permissions.Count() != (type as PrtInterfaceType).permissions.Count())
                    {
                        return false;
                    }
                    else
                    {
                        foreach(var ev in interValue.permissions)
                        {
                            if (!(type as PrtInterfaceType).permissions.Contains(ev))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            else if (type is PrtNamedTupleType) // must come before PrtTupleType since PrtNamedTupleType derives from PrtTupleType
            {
                var nmtupType = type as PrtNamedTupleType;
                var nmtupVal = value as PrtNamedTupleValue;
                if (nmtupVal == null) return false;
                if (nmtupVal.fieldValues.Count != nmtupType.fieldTypes.Count) return false;
                for (int i = 0; i < nmtupVal.fieldValues.Count; i++)
                {
                    if (nmtupVal.fieldNames[i] != nmtupType.fieldNames[i]) return false;
                }
                for (int i = 0; i < nmtupVal.fieldValues.Count; i++)
                {
                    if (!PrtInhabitsType(nmtupVal.fieldValues[i], nmtupType.fieldTypes[i])) return false;
                }
                return true;
            }
            else if (type is PrtTupleType)
            {
                var tupType = type as PrtTupleType;
                var tupVal = value as PrtTupleValue;
                if (tupVal == null) return false;
                if (tupVal.fieldValues.Count != tupType.fieldTypes.Count) return false;
                for (int i = 0; i < tupVal.fieldValues.Count; i++)
                {
                    if (!PrtInhabitsType(tupVal.fieldValues[i], tupType.fieldTypes[i])) return false;
                }
                return true;
            }
            else if (type is PrtMapType)
            {
                var mapType = type as PrtMapType;
                var mapVal = value as PrtMapValue;
                if (mapVal == null) return false;
                foreach (var p in mapVal.keyToValueMap)
                {
                    if (!PrtInhabitsType(p.Key.key, mapType.keyType)) return false;
                    if (!PrtInhabitsType(p.Value, mapType.valType)) return false;
                }
                return true;
            }
            else if (type is PrtSeqType)
            {
                var seqType = type as PrtSeqType;
                var seqVal = value as PrtSeqValue;
                if (seqVal == null) return false;
                foreach (var elem in seqVal.elements)
                {
                    if (!PrtInhabitsType(elem, seqType.elemType)) return false;
                }
                return true;
            }
            else
            {
                throw new PrtInternalException("Unknown type in PrtInhabitsType");
            }
        }

        public static PrtValue PrtConvertValue(PrtValue value, PrtType type)
        {
            //cast for interface types is implemented as reduce.
            if (type is PrtInterfaceType)
            {
                return (type as PrtInterfaceType).PrtReduceValue(value);
            }
            else if (type is PrtIntType)
            {
                if(value is PrtIntValue)
                {
                    return (new PrtIntValue((value as PrtIntValue).nt));
                }
                else
                {
                    return (new PrtIntValue((Int64)(value as PrtFloatValue).ft));
                }
            }
            else if (type is PrtFloatType)
            {
                if (value is PrtIntValue)
                {
                    return (new PrtFloatValue((value as PrtIntValue).nt));
                }
                else
                {
                    return (new PrtFloatValue((Int64)(value as PrtFloatValue).ft));
                }
            }
            else
            {
                throw new PrtInternalException("unexpected type in convert operation");
            }
        }
        public static PrtValue PrtCastValue(PrtValue value, PrtType type)
        {
            if (!PrtInhabitsType(value, type))
                throw new PrtInhabitsTypeException(String.Format("value {0} is not a member of type {1}", value.ToString(), type.ToString()));
            return value.Clone();
        }

        public virtual void Resolve(StateImpl state) { }
    }

    [Serializable]
    public class PrtIntValue : PrtValue
    {
        public Int64 nt;

        public PrtIntValue()
        {
            nt = 0;
        }

        public PrtIntValue(Int64 val)
        {
            nt = val;
        }

        public override PrtValue Clone()
        {
            return new PrtIntValue(this.nt);
        }

        public override bool Equals(object val)
        {
            var intVal = val as PrtIntValue;
            if (intVal == null) return false;
            return this.nt == intVal.nt;
        }

        public override int GetHashCode()
        {
            return nt.GetHashCode();
        }
        public override string ToString()
        {
            return nt.ToString();
        }
    }

    [Serializable]
    public class PrtFloatValue : PrtValue
    {
        public double ft;

        public PrtFloatValue()
        {
            ft = 0;
        }

        public PrtFloatValue(double val)
        {
            ft = val;
        }

        public override PrtValue Clone()
        {
            return new PrtFloatValue(this.ft);
        }

        public override bool Equals(object val)
        {
            var ftVal = val as PrtFloatValue;
            if (ftVal == null) return false;
            return this.ft == ftVal.ft;
        }

        public override int GetHashCode()
        {
            return ft.GetHashCode();
        }
        public override string ToString()
        {
            return ft.ToString();
        }
    }

    [Serializable]
    public class PrtBoolValue : PrtValue
    {
        public bool bl;

        public PrtBoolValue()
        {
            bl = false;
        }

        public PrtBoolValue(bool val)
        {
            this.bl = val;
        }

        public override PrtValue Clone()
        {
            return new PrtBoolValue(this.bl);
        }

        public override bool Equals(object val)
        {
            var boolVal = val as PrtBoolValue;
            if (boolVal == null) return false;
            return this.bl == boolVal.bl;
        }

        public override int GetHashCode()
        {
            return bl.GetHashCode();
        }
        public override string ToString()
        {
            return bl.ToString();
        }
    }

    [Serializable]
    public class PrtEventValue : PrtValue
    {
        public PrtEvent evt;

        public PrtEventValue(PrtEvent val)
        {
            this.evt = val;
        }

        public override PrtValue Clone()
        {
            return new PrtEventValue(this.evt);
        }

        public override bool Equals(object val)
        {
            var eventVal = val as PrtEventValue;
            if (eventVal == null) return false;
            return this.evt.name == eventVal.evt.name;
        }

        public override int GetHashCode()
        {
            return evt.GetHashCode();
        }

        public override string ToString()
        {
            return evt.name;
        }
    }

    [Serializable]
    public class PrtEnumValue : PrtIntValue
    {
        public string constName;

        public PrtEnumValue(string name, Int64 val) : base(val)
        {
            constName = name;
        }

        public override PrtValue Clone()
        {
            return new PrtEnumValue(this.constName, this.nt);
        }

    }

    [Serializable]
    public class PrtInterfaceValue : PrtMachineValue
    {
        public List<PrtEventValue> permissions;

        public PrtInterfaceValue(PrtImplMachine m, List<PrtEventValue> perm): base(m)
        {
            if (perm == null)
                permissions = null;
            else
            {
                permissions = new List<PrtEventValue>();
                foreach (var ev in perm)
                {
                    permissions.Add(ev);
                }
            }
            
        }

        public override PrtValue Clone()
        {
            return new PrtInterfaceValue(mach, permissions);
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", mach.renamedName, mach.instanceNumber);
        }

    }

    [Serializable]
    public class PrtMachineValue : PrtValue
    {
        public PrtImplMachine mach;

        public PrtMachineValue(PrtImplMachine mach)
        {
            this.mach = mach;
        }

        public override PrtValue Clone()
        {
            return new PrtMachineValue(this.mach);
        }

        public override void Resolve(StateImpl state)
        {
            mach = state.ImplMachines.First(m => m.renamedName == mach.renamedName && m.instanceNumber == mach.instanceNumber);
        }

        public override bool Equals(object val)
        {
            var machineVal = val as PrtMachineValue;
            if (machineVal == null) return false;
            return this.mach.renamedName == machineVal.mach.renamedName && this.mach.instanceNumber == machineVal.mach.instanceNumber;
        }

        public override int GetHashCode()
        {
            //return mach.GetHashCode(); 
            return Hashing.Hash(mach.renamedName.GetHashCode(), mach.instanceNumber.GetHashCode());
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", mach.Name, mach.instanceNumber);
        }
    }

    [Serializable]
    public class PrtTupleValue : PrtValue
    {
        public List<PrtValue> fieldValues;

        public PrtTupleValue()
        {
            fieldValues = new List<PrtValue>();
        }

        public PrtTupleValue(PrtType type)
        {
            var tupType = type as PrtTupleType;
            fieldValues = new List<PrtValue>(tupType.fieldTypes.Count);
            foreach (var ft in tupType.fieldTypes)
            {
                fieldValues.Add(PrtMkDefaultValue(ft));
            }
        }

        public PrtTupleValue(params PrtValue[] elems)
        {
            fieldValues = new List<PrtValue>(elems.Count());
            foreach (var elem in elems)
            {
                fieldValues.Add(elem.Clone());
            }
        }

        public void Update(int index, PrtValue val)
        {
            fieldValues[index] = val;
        }

        public PrtValue UpdateAndReturnOldValue(int index, PrtValue val)
        {
            var oldVal = fieldValues[index];
            fieldValues[index] = val;
            return oldVal;
        }

        public override PrtValue Clone()
        {
            var clone = new PrtTupleValue();
            foreach (var val in fieldValues)
            {
                clone.fieldValues.Add(val.Clone());
            }
            return clone;
        }

        public override bool Equals(object val)
        {
            if (val is PrtNamedTupleValue) return false;
            var tupValue = (val as PrtTupleValue);
            if (tupValue == null) return false;
            if (tupValue.fieldValues.Count != this.fieldValues.Count) return false;
            for (int i = 0;  i < fieldValues.Count; i++)
            {
                if (!this.fieldValues[i].Equals(tupValue.fieldValues[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return fieldValues.Select(v => v.GetHashCode()).Hash();
        }

        public override void Resolve(StateImpl state)
        {
            fieldValues.ForEach(f => f.Resolve(state));
        }

        public override string ToString()
        {
            string retStr = "<";
            foreach (var field in fieldValues)
            {
                retStr = retStr + field.ToString() + ",";
            }
            retStr += ">";
            return retStr;
        }
    }

    [Serializable]
    public class PrtNamedTupleValue : PrtTupleValue
    {
        public List<string> fieldNames;

        public PrtNamedTupleValue() : base()
        {
            fieldNames = new List<string>();
        }

        public PrtNamedTupleValue(PrtType type) : base (type)
        {
            var tupType = type as PrtNamedTupleType;
            fieldNames = new List<string>(tupType.fieldTypes.Count);
            foreach (var fn in tupType.fieldNames)
            {
                fieldNames.Add(fn);
            }
        }

        public PrtNamedTupleValue(PrtType type, params PrtValue[] elems) : base (elems)
        {
            var tupType = type as PrtNamedTupleType;
            fieldNames = new List<string>(tupType.fieldTypes.Count);
            foreach (var fn in tupType.fieldNames)
            {
                fieldNames.Add(fn);
            }
        }

        public override PrtValue Clone()
        {
            var clone = new PrtNamedTupleValue();
            foreach (var name in fieldNames)
            {
                clone.fieldNames.Add(name);
            }
            foreach (var val in fieldValues)
            {
                clone.fieldValues.Add(val.Clone());
            }
            return clone;
        }

        public override bool Equals(object val)
        {
            var tup = val as PrtNamedTupleValue;
            if (tup == null) return false;
            if (tup.fieldValues.Count != this.fieldValues.Count) return false;
            for (int i = 0; i < tup.fieldValues.Count; i++)
            {
                if (this.fieldNames[i] != tup.fieldNames[i]) return false;
                if (!this.fieldValues[i].Equals(tup.fieldValues[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(base.GetHashCode(), fieldValues.Select(s => s.GetHashCode()).Hash()); 
        }

        public override string ToString()
        {
            string retStr = "<";
            for (int i = 0; i < fieldValues.Count; i++)
            {
                retStr += fieldNames[i] + ":" + fieldValues[i].ToString() + ", ";
            }
            retStr += ">";
            return retStr;
        }
    }

    [Serializable]
    public class PrtSeqValue : PrtValue
    {
        public List<PrtValue> elements;

        public PrtSeqValue()
        {
            elements = new List<PrtValue>();
        }

        public override PrtValue Clone()
        {
            var clone = new PrtSeqValue();
            foreach (var val in elements)
            {
                clone.elements.Add(val.Clone());
            }
            return clone;
        }

        public PrtValue Lookup(Int64 index)
        {
            if (index < 0 || index >= elements.Count)
            {
                throw new PrtAssertFailureException("Illegal index for Lookup");
            }
            return elements[(int)index];
        }

        public PrtValue Lookup(PrtValue index)
        {
            return Lookup(((PrtIntValue)index).nt);
        }

        public void Insert(Int64 index, PrtValue val)
        {
            if (index < 0 || index > elements.Count)
            {
                throw new PrtAssertFailureException("Illegal index for Insert");
            }
            elements.Insert((int)index, val);
        }

        public void Insert(PrtValue index, PrtValue val)
        {
            Insert(((PrtIntValue)index).nt, val);
        }

        public void Update(int index, PrtValue val)
        {
            if (index < 0 || index > elements.Count)
            {
                throw new PrtAssertFailureException("Illegal index for Update");
            }
            if (index == elements.Count)
            {
                elements.Insert(index, val);
            }
            else
            {
                elements[index] = val;
            }
        }

        public void Update(PrtValue index, PrtValue val)
        {
            Update((int)((PrtIntValue)index).nt, val);
        }

        public PrtValue UpdateAndReturnOldValue(PrtValue index, PrtValue val)
        {
            var oldVal = Lookup(index);
            Update(index, val);
            return oldVal;
        }

        public void Remove(Int64 index)
        {
            if (index < 0 || index >= elements.Count)
            {
                throw new PrtAssertFailureException("Illegal index for Remove");
            }
            elements.RemoveAt((int)index);
        }

        public void Remove(PrtValue index)
        {
            Remove(((PrtIntValue)index).nt);
        }

        public override int Size()
        {
            return elements.Count();
        }

        public override bool Equals(object val)
        {
            var seqVal = val as PrtSeqValue;
            if (seqVal == null) return false;
            if (this.elements.Count != seqVal.elements.Count) return false;
            for (int i = 0; i < this.elements.Count; i++)
            {
                if (!this.elements[i].Equals(seqVal.elements[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return elements.Select(v => v.GetHashCode()).Hash(); 
        }

        public override void Resolve(StateImpl state)
        {
            elements.ForEach(f => f.Resolve(state));
        }

        public override string ToString()
        {
            string retStr = "(";
            for (int i = 0; i < elements.Count; i++)
            {
                retStr += elements[i] + ", ";
            }
            retStr += ")";
            return retStr;
        }
    }

    [Serializable]
    public class PrtMapKey
    {
        public PrtValue key;
        public int keyIndex;
        public PrtMapKey(PrtValue x, int i)
        {
            key = x;
            keyIndex = i;
        }
        public override bool Equals(object obj)
        {
            var mapKey = obj as PrtMapKey;
            if (mapKey == null) return false;
            return key.Equals(mapKey.key);
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(key.GetHashCode(), keyIndex.GetHashCode());
        }

        public void Resolve(StateImpl state)
        {
            key.Resolve(state);
        }
    }

    [Serializable]
    public class PrtMapValue : PrtValue
    {
        public int nextKeyIndex;
        public Dictionary<PrtMapKey, PrtValue> keyToValueMap;

        public PrtMapValue()
        {
            nextKeyIndex = 0;
            keyToValueMap = new Dictionary<PrtMapKey, PrtValue>();
        }

        public override PrtValue Clone()
        {
            var clone = new PrtMapValue();
            int count = 0;
            foreach (var k in keyToValueMap.Keys.OrderBy(x => x.keyIndex))
            {
                clone.keyToValueMap[new PrtMapKey(k.key.Clone(), count)] = keyToValueMap[k].Clone();
                count++;
            }
            clone.nextKeyIndex = count;
            return clone;
        }

        public override int Size()
        {
            return keyToValueMap.Count;
        }

        public PrtValue Lookup(PrtValue key)
        {
            if (!Contains(key))
            {
                throw new PrtAssertFailureException(string.Format("Illegal key ({0}) in Lookup", key.ToString()));
            }
            return keyToValueMap[new PrtMapKey(key, 0)];
        }

        public bool Contains(PrtValue key)
        {
            return keyToValueMap.ContainsKey(new PrtMapKey(key, 0));
        }

        public PrtSeqValue Keys()
        {
            var seqKeys = new PrtSeqValue();
            foreach (var k in keyToValueMap.Keys.OrderBy(x => x.keyIndex))
            {
                seqKeys.elements.Add(k.key.Clone());
            }
            return seqKeys;
        }

        public PrtSeqValue Values()
        {
            var seqValues = new PrtSeqValue();
            foreach (var k in keyToValueMap.Keys.OrderBy(x => x.keyIndex))
            {
                seqValues.elements.Add(keyToValueMap[k].Clone());
            }
            return seqValues;
        }

        public void Remove(PrtValue key)
        {
            if (!Contains(key))
            {
                throw new PrtAssertFailureException("Illegal key in Remove");
            }
            keyToValueMap.Remove(new PrtMapKey(key, 0));
        }

        public void Update(PrtValue key, PrtValue val)
        {
            keyToValueMap[new PrtMapKey(key, nextKeyIndex)] = val;
            nextKeyIndex++;
        }

        public PrtValue UpdateAndReturnOldValue(PrtValue key, PrtValue val)
        {
            var oldVal = Lookup(key);
            Update(key, val);
            return oldVal;
        }

        public override bool Equals(object val)
        {
            var mapVal = val as PrtMapValue;
            if (mapVal == null) return false;
            if (this.keyToValueMap.Count != mapVal.keyToValueMap.Count) return false;
            foreach (var k in this.keyToValueMap.Keys)
            {
                if (!mapVal.keyToValueMap.ContainsKey(k)) return false;
                if (!this.keyToValueMap[k].Equals(mapVal.keyToValueMap[k])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            //return keyToValueMap.GetHashCode();
            return Hashing.Hash(nextKeyIndex.GetHashCode(),
                keyToValueMap.Select(tup => Hashing.Hash(tup.Key.GetHashCode(), tup.Value.GetHashCode())).Hash());
        }

        public override void Resolve(StateImpl state)
        {
            var list = keyToValueMap.ToList();
            list.ForEach(tup =>
            {
                tup.Key.Resolve(state);
                tup.Value.Resolve(state);
            });
            keyToValueMap = new Dictionary<PrtMapKey, PrtValue>();
            list.ForEach(tup => keyToValueMap.Add(tup.Key, tup.Value));
        }

        public override string ToString()
        {
            string retStr = "(";
            foreach (var k in keyToValueMap.Keys.OrderBy(x => x.keyIndex))
            {
                retStr += "(" + k.key.ToString() + "," + keyToValueMap[k].ToString() + "), ";
            }
            retStr += ")";
            return retStr;
        }
    }
}