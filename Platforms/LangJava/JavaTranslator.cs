﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;
using Common;

namespace LangJava
{
    public abstract class JavaTranslator : Platform.CurlyBraceTranslator
    {
        public JavaTranslator(Platform.AbstractPlatform platform) : base(platform, "  ", "\n", true)
        { }

        public override void TranslateArrayGet(StringBuilder sb, Expression array, Expression index)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append(']');
        }

        public override void TranslateArrayLength(StringBuilder sb, Expression array)
        {
            this.TranslateExpression(sb, array);
            sb.Append(".length");
        }

        public override void TranslateArrayNew(StringBuilder sb, PType arrayType, Expression lengthExpression)
        {
            // In the event of multi-dimensional jagged arrays, the outermost array length goes in the innermost bracket.
            // Unwrap nested arrays in the type and run the code as normal, and then add that many []'s to the end.
            int bracketSuffixCount = 0;
            while (arrayType.RootValue == "Array")
            {
                arrayType = arrayType.Generics[0];
                bracketSuffixCount++;
            }

            sb.Append("new ");
            sb.Append(this.Platform.TranslateType(arrayType));
            sb.Append('[');
            this.TranslateExpression(sb, lengthExpression);
            sb.Append(']');

            while (bracketSuffixCount-- > 0)
            {
                sb.Append("[]");
            }
        }

        public override void TranslateArraySet(StringBuilder sb, Expression array, Expression index, Expression value)
        {
            this.TranslateExpression(sb, array);
            sb.Append('[');
            this.TranslateExpression(sb, index);
            sb.Append("] = ");
            this.TranslateExpression(sb, value);
        }

        public override void TranslateCast(StringBuilder sb, PType type, Expression expression)
        {
            sb.Append("((");
            sb.Append(this.Platform.TranslateType(type));
            sb.Append(") ");
            this.TranslateExpression(sb, expression);
            sb.Append(')');
        }

        public override void TranslateCharConstant(StringBuilder sb, char value)
        {
            sb.Append(Util.ConvertCharToCharConstantCode(value));
        }

        public override void TranslateCharToString(StringBuilder sb, Expression charValue)
        {
            sb.Append("(\"\" + ");
            this.TranslateExpression(sb, charValue);
            sb.Append(')');
        }

        public override void TranslateChr(StringBuilder sb, Expression charCode)
        {
            sb.Append("Character.toString((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateConstructorInvocation(StringBuilder sb, ConstructorInvocation constructorInvocation)
        {
            sb.Append("new ");
            sb.Append(constructorInvocation.StructType.NameToken.Value);
            sb.Append('(');
            Expression[] args = constructorInvocation.Args;
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) sb.Append(", ");
                this.TranslateExpression(sb, args[i]);
            }
            sb.Append(')');
        }

        // TODO: you really should rename this
        public override void TranslateConvertRawDictionaryValueCollectionToAReusableValueList(StringBuilder sb, Expression dictionary)
        {
            sb.Append("new ArrayList<>(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(')');
        }

        public override void TranslateCurrentTimeSeconds(StringBuilder sb)
        {
            sb.Append("System.currentTimeMillis() / 1000.0");
        }

        public override void TranslateDictionaryContainsKey(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".containsKey(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryGet(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".get(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionaryKeys(StringBuilder sb, Expression dictionary)
        {
            sb.Append("TranslationHelper.convert");
            switch (dictionary.ResolvedType.Generics[0].RootValue)
            {
                case "int": sb.Append("Integer"); break;
                case "string": sb.Append("String"); break;

                // TODO: explicitly disallow this at compile-time
                default: throw new NotImplementedException();
            }
            sb.Append("SetToArray(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".keySet())");

            // TODO: do a simple .keySet().toArray(TranslationHelper.STATIC_INSTANCE_OF_ZERO_LENGTH_INT_OR_STRING_ARRAY);
        }

        public override void TranslateDictionaryKeysToValueList(StringBuilder sb, Expression dictionary)
        {
            throw new NotImplementedException();
        }

        public override void TranslateDictionaryNew(StringBuilder sb, PType keyType, PType valueType)
        {
            sb.Append("new HashMap<>()");
        }

        public override void TranslateDictionaryRemove(StringBuilder sb, Expression dictionary, Expression key)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".remove(");
            this.TranslateExpression(sb, key);
            sb.Append(')');
        }

        public override void TranslateDictionarySet(StringBuilder sb, Expression dictionary, Expression key, Expression value)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".put(");
            this.TranslateExpression(sb, key);
            sb.Append(", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateDictionarySize(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".size()");
        }

        public override void TranslateDictionaryValues(StringBuilder sb, Expression dictionary)
        {
            this.TranslateExpression(sb, dictionary);
            sb.Append(".values()");
        }

        public override void TranslateDictionaryValuesToValueList(StringBuilder sb, Expression dictionary)
        {
            sb.Append("new ArrayList<>(");
            this.TranslateExpression(sb, dictionary);
            sb.Append(".values())");
        }

        public override void TranslateFloatBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.FLOAT_BUFFER_16");
        }

        public override void TranslateFloatDivision(StringBuilder sb, Expression floatNumerator, Expression floatDenominator)
        {
            this.TranslateExpression(sb, floatNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, floatDenominator);
        }

        public override void TranslateFloatToInt(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFloatToString(StringBuilder sb, Expression floatExpr)
        {
            sb.Append("Double.toString(");
            this.TranslateExpression(sb, floatExpr);
            sb.Append(')');
        }

        public override void TranslateFunctionInvocationInterpreterScoped(StringBuilder sb, FunctionReference funcRef, Expression[] args)
        {
            sb.Append("Interpreter.");
            base.TranslateFunctionInvocationInterpreterScoped(sb, funcRef, args);
        }

        public override void TranslateGetProgramData(StringBuilder sb)
        {
            sb.Append("TranslationHelper.getProgramData()");
        }

        public override void TranslateGetResourceManifest(StringBuilder sb)
        {
            sb.Append("AwtTranslationHelper.getResourceManifest()");
        }

        public override void TranslateGlobalVariable(StringBuilder sb, Variable variable)
        {
            sb.Append("VmGlobal.");
            sb.Append(variable.Name);
        }

        public override void TranslateIntBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.INT_BUFFER_16");
        }

        public override void TranslateIntegerDivision(StringBuilder sb, Expression integerNumerator, Expression integerDenominator)
        {
            this.TranslateExpression(sb, integerNumerator);
            sb.Append(" / ");
            this.TranslateExpression(sb, integerDenominator);
        }

        public override void TranslateIntToString(StringBuilder sb, Expression integer)
        {
            sb.Append("Integer.toString(");
            this.TranslateExpression(sb, integer);
            sb.Append(')');
        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            sb.Append("TranslationHelper.invokeLibraryFunction(");
            this.TranslateExpression(sb, functionId);
            sb.Append(", ");
            this.TranslateExpression(sb, argsArray);
            sb.Append(')');
        }

        public override void TranslateIsValidInteger(StringBuilder sb, Expression stringValue)
        {
            sb.Append("TranslationHelper.isValidInteger(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateListAdd(StringBuilder sb, Expression list, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".add(");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListClear(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".clear()");
        }

        public override void TranslateListConcat(StringBuilder sb, Expression list, Expression items)
        {
            sb.Append("TranslationHelper.concatLists(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, items);
            sb.Append(')');
        }

        public override void TranslateListGet(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".get(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListInsert(StringBuilder sb, Expression list, Expression index, Expression item)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".insert(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, item);
            sb.Append(')');
        }

        public override void TranslateListJoinChars(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper.joinChars(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListJoinStrings(StringBuilder sb, Expression list, Expression sep)
        {
            sb.Append("TranslationHelper.joinList(");
            this.TranslateExpression(sb, sep);
            sb.Append(", ");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListNew(StringBuilder sb, PType type)
        {
            sb.Append("new ArrayList<");
            sb.Append(LangJava.PlatformImpl.TranslateJavaNestedType(type));
            sb.Append(">()");
        }

        public override void TranslateListPop(StringBuilder sb, Expression list)
        {
            // TODO: make this check a little more lenient with other simple patterns, such as struct fields on variables, etc
            if (list is Variable)
            {
                this.TranslateExpression(sb, list);
                sb.Append(".remove(");
                this.TranslateExpression(sb, list);
                sb.Append(".size() - 1)");
            }
            else
            {
                sb.Append("TranslationHelper.listPop(");
                this.TranslateExpression(sb, list);
                sb.Append(')');
            }
        }

        public override void TranslateListRemoveAt(StringBuilder sb, Expression list, Expression index)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".remove(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateListReverse(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper.reverseList(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSet(StringBuilder sb, Expression list, Expression index, Expression value)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".set(");
            this.TranslateExpression(sb, index);
            sb.Append(", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateListShuffle(StringBuilder sb, Expression list)
        {
            sb.Append("TranslationHelper.shuffleInPlace(");
            this.TranslateExpression(sb, list);
            sb.Append(')');
        }

        public override void TranslateListSize(StringBuilder sb, Expression list)
        {
            this.TranslateExpression(sb, list);
            sb.Append(".size()");
        }

        public override void TranslateListToArray(StringBuilder sb, Expression list)
        {
            string typeString = this.Platform.TranslateType(list.ResolvedType.Generics[0]);
            bool doubleEval = true;
            switch (typeString)
            {
                case "boolean":
                case "int":
                case "double":
                case "String":
                case "Value":
                    doubleEval = false;
                    break;
            }
            this.TranslateExpression(sb, list);
            sb.Append(".toArray(");
            if (typeString == "Object")
            {
                sb.Append(')');
            }
            else if (doubleEval)
            {
                sb.Append("new ");
                sb.Append(typeString);
                sb.Append('[');
                this.TranslateExpression(sb, list);
                sb.Append(".size()])");

                // I'm just curious what sort of other common expressions are used
                string name = list.GetType().Name;
                switch (name)
                {
                    case "DotField":
                        break;
                    default:
                        CompatibilityHack.CriticalTODO("get rid of this");
                        throw new NotImplementedException(name);
                }
            }
            else
            {
                sb.Append("TranslationHelper.EMPTY_ARRAY_" + typeString.ToUpper());
                sb.Append(')');
            }
        }

        public override void TranslateMathArcCos(StringBuilder sb, Expression ratio)
        {
            sb.Append("Math.acos(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcSin(StringBuilder sb, Expression ratio)
        {
            sb.Append("Math.asin(");
            this.TranslateExpression(sb, ratio);
            sb.Append(')');
        }

        public override void TranslateMathArcTan(StringBuilder sb, Expression yComponent, Expression xComponent)
        {
            sb.Append("Math.atan2(");
            this.TranslateExpression(sb, yComponent);
            sb.Append(", ");
            this.TranslateExpression(sb, xComponent);
            sb.Append(')');
        }

        public override void TranslateMathCos(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.cos(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathLog(StringBuilder sb, Expression value)
        {
            sb.Append("Math.log(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateMathPow(StringBuilder sb, Expression expBase, Expression exponent)
        {
            sb.Append("Math.pow(");
            this.TranslateExpression(sb, expBase);
            sb.Append(", ");
            this.TranslateExpression(sb, exponent);
            sb.Append(')');
        }

        public override void TranslateMathSin(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.sin(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMathTan(StringBuilder sb, Expression thetaRadians)
        {
            sb.Append("Math.tan(");
            this.TranslateExpression(sb, thetaRadians);
            sb.Append(')');
        }

        public override void TranslateMultiplyList(StringBuilder sb, Expression list, Expression n)
        {
            sb.Append("TranslationHelper.multiplyList(");
            this.TranslateExpression(sb, list);
            sb.Append(", ");
            this.TranslateExpression(sb, n);
            sb.Append(')');
        }

        public override void TranslateNullConstant(StringBuilder sb)
        {
            sb.Append("null");
        }

        public override void TranslateParseFloatUnsafe(StringBuilder sb, Expression stringValue)
        {
            sb.Append("Double.toString(");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateParseInt(StringBuilder sb, Expression safeStringValue)
        {
            sb.Append("Integer.toString(");
            this.TranslateExpression(sb, safeStringValue);
            sb.Append(')');
        }

        public override void TranslateRandomFloat(StringBuilder sb)
        {
            sb.Append("TranslationHelper.random.nextDouble()");
        }

        public override void TranslateRegisterLibraryFunction(StringBuilder sb, Expression libRegObj, Expression functionName, Expression functionArgCount)
        {
            sb.Append("TranslationHelper.registerLibraryFunction(LibraryWrapper.class, ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(", ");
            this.TranslateExpression(sb, functionName);
            sb.Append(", ");
            this.TranslateExpression(sb, functionArgCount);
            sb.Append(')');
        }

        public override void TranslateResourceReadTextFile(StringBuilder sb, Expression path)
        {
            sb.Append("ResourceReader.readTextFile(");
            this.TranslateExpression(sb, path);
            sb.Append(')');
        }

        public override void TranslateSetProgramData(StringBuilder sb, Expression programData)
        {
            sb.Append("TranslationHelper.setProgramData(");
            this.TranslateExpression(sb, programData);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfIntArray(StringBuilder sb, Expression intArray)
        {
            sb.Append("TranslationHelper.sortedCopyOfIntArray(");
            this.TranslateExpression(sb, intArray);
            sb.Append(')');
        }

        public override void TranslateSortedCopyOfStringArray(StringBuilder sb, Expression stringArray)
        {
            sb.Append("TranslationHelper.sortedCopyOfStringArray(");
            this.TranslateExpression(sb, stringArray);
            sb.Append(')');
        }

        public override void TranslateStringAppend(StringBuilder sb, Expression str1, Expression str2)
        {
            this.TranslateExpression(sb, str1);
            sb.Append(" += ");
            this.TranslateExpression(sb, str2);
        }

        public override void TranslateStringBuffer16(StringBuilder sb)
        {
            sb.Append("TranslationHelper.STRING_BUFFER_16");
        }

        public override void TranslateStringCharAt(StringBuilder sb, Expression str, Expression index)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append(')');
        }

        public override void TranslateStringCharCodeAt(StringBuilder sb, Expression str, Expression index)
        {
            sb.Append("((int) ");
            this.TranslateExpression(sb, str);
            sb.Append(".charAt(");
            this.TranslateExpression(sb, index);
            sb.Append("))");
        }

        public override void TranslateStringCompareIsReverse(StringBuilder sb, Expression str1, Expression str2)
        {
            sb.Append('(');
            this.TranslateExpression(sb, str1);
            sb.Append(".compareTo(");
            this.TranslateExpression(sb, str2);
            sb.Append(") > 0)");
        }

        public override void TranslateStringConcatAll(StringBuilder sb, Expression[] strings)
        {
            this.TranslateExpression(sb, strings[0]);
            for (int i = 1; i < strings.Length; ++i)
            {
                sb.Append(" + ");
                this.TranslateExpression(sb, strings[i]);
            }
        }

        public override void TranslateStringConcatPair(StringBuilder sb, Expression strLeft, Expression strRight)
        {
            this.TranslateExpression(sb, strLeft);
            sb.Append(" + ");
            this.TranslateExpression(sb, strRight);
        }

        public override void TranslateStringContains(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".contains(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEndsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".endsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringEquals(StringBuilder sb, Expression left, Expression right)
        {
            this.TranslateExpression(sb, left);
            sb.Append(".equals(");
            this.TranslateExpression(sb, right);
            sb.Append(')');
        }

        public override void TranslateStringFromCharCode(StringBuilder sb, Expression charCode)
        {
            sb.Append("Character.toString((char) ");
            this.TranslateExpression(sb, charCode);
            sb.Append(")");
        }

        public override void TranslateStringIndexOf(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringIndexOfWithStart(StringBuilder sb, Expression haystack, Expression needle, Expression startIndex)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".indexOf(");
            this.TranslateExpression(sb, needle);
            sb.Append(", ");
            this.TranslateExpression(sb, startIndex);
            sb.Append(')');
        }

        public override void TranslateStringLength(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".length()");
        }

        public override void TranslateStringReplace(StringBuilder sb, Expression haystack, Expression needle, Expression newNeedle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".replace((CharSequence) ");
            this.TranslateExpression(sb, needle);
            sb.Append(", (CharSequence) ");
            this.TranslateExpression(sb, newNeedle);
            sb.Append(')');
        }

        public override void TranslateStringReverse(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.reverseString(");
            this.TranslateExpression(sb, str);
            sb.Append(')');
        }

        public override void TranslateStringSplit(StringBuilder sb, Expression haystack, Expression needle)
        {
            sb.Append("TranslationHelper.literalStringSplit(");
            this.TranslateExpression(sb, haystack);
            sb.Append(", ");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringStartsWith(StringBuilder sb, Expression haystack, Expression needle)
        {
            this.TranslateExpression(sb, haystack);
            sb.Append(".startsWith(");
            this.TranslateExpression(sb, needle);
            sb.Append(')');
        }

        public override void TranslateStringToLower(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toLowerCase()");
        }

        public override void TranslateStringToUpper(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".toUpperCase()");
        }

        public override void TranslateStringTrim(StringBuilder sb, Expression str)
        {
            this.TranslateExpression(sb, str);
            sb.Append(".trim()");
        }

        public override void TranslateStringTrimEnd(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.trimSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", false)");
        }

        public override void TranslateStringTrimStart(StringBuilder sb, Expression str)
        {
            sb.Append("TranslationHelper.trimSide(");
            this.TranslateExpression(sb, str);
            sb.Append(", true)");
        }

        public override void TranslateStrongReferenceEquality(StringBuilder sb, Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public override void TranslateStructFieldDereference(StringBuilder sb, Expression root, StructDefinition structDef, string fieldName, int fieldIndex)
        {
            this.TranslateExpression(sb, root);
            sb.Append('.');
            sb.Append(fieldName);
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            sb.Append("TranslationHelper.sleep(");
            this.TranslateExpression(sb, seconds);
            sb.Append(')');
        }

        public override void TranslateTryParseFloat(StringBuilder sb, Expression stringValue, Expression floatOutList)
        {
            sb.Append("TranslationHelper.parseFloatOrReturnNull(");
            this.TranslateExpression(sb, floatOutList);
            sb.Append(", ");
            this.TranslateExpression(sb, stringValue);
            sb.Append(')');
        }

        public override void TranslateVariableDeclaration(StringBuilder sb, VariableDeclaration varDecl)
        {
            sb.Append(this.CurrentTab);
            sb.Append(this.Platform.TranslateType(varDecl.Type));
            sb.Append(" v_");
            sb.Append(varDecl.VariableNameToken.Value);
            if (varDecl.Value != null)
            {
                sb.Append(" = ");
                this.TranslateExpression(sb, varDecl.Value);
            }
            sb.Append(';');
            sb.Append(this.NewLine);
        }

        public override void TranslateVmDetermineLibraryAvailability(StringBuilder sb, Expression libraryName, Expression libraryVersion)
        {
            sb.Append("TranslationHelper.checkLibraryAvaialability(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libraryVersion);
            sb.Append(')');
        }

        public override void TranslateVmEnqueueResume(StringBuilder sb, Expression seconds, Expression executionContextId)
        {
            throw new NotImplementedException();
        }

        public override void TranslateVmRunLibraryManifest(StringBuilder sb, Expression libraryName, Expression libRegObj)
        {
            sb.Append("TranslationHelper.runLibraryManifest(");
            this.TranslateExpression(sb, libraryName);
            sb.Append(", ");
            this.TranslateExpression(sb, libRegObj);
            sb.Append(')');
        }
    }
}
