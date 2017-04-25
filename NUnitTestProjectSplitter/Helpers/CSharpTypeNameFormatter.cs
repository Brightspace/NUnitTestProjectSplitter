using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NUnitTestProjectSplitter.Helpers {

	public class CSharpTypeNameFormatter {

		private static readonly Regex m_genTypeDefName = new Regex(
				@"^(?<name>.+)`\d+$",
				RegexOptions.Compiled | RegexOptions.Singleline
			);

		private readonly bool m_useKeywordTypeNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="CSharpTypeNameFormatter"/> class.
		/// </summary>
		public CSharpTypeNameFormatter()
			: this( false ) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CSharpTypeNameFormatter"/> class.
		/// </summary>
		/// <param name="doNotUseKeywordTypeNames">if set to <c>true</c> [do not use keyword type names].</param>
		public CSharpTypeNameFormatter( bool doNotUseKeywordTypeNames ) {
			m_useKeywordTypeNames = !( doNotUseKeywordTypeNames );
		}

		/// <summary>
		/// Formats the C# type name.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>Returns the formatted type name.</returns>
		public string FormatName( Type type ) {

			StringBuilder sb = new StringBuilder();
			FormatName( type, sb );
			return sb.ToString();
		}

		/// <summary>
		/// Formats the full C# type name.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>Returns the full formatted type name.</returns>
		public string FormatFullName( Type type ) {

			StringBuilder sb = new StringBuilder();
			FormatFullName( type, sb );
			return sb.ToString();
		}

		private void FormatArrayDefinition( Type type, StringBuilder sb ) {

			sb.Append( "[" );

			int rank = type.GetArrayRank();
			for( int i = 1; i < rank; i++ ) {
				sb.Append( "," );
			}

			sb.Append( "]" );
		}

		private void FormatFullName( Type type, StringBuilder sb ) {

			if( type.IsArray ) {
				FormatFullName( type.GetElementType(), sb );
				FormatArrayDefinition( type, sb );
			} else {

				bool hasKeyword = HasKeywordTypeName( type );
				if( !hasKeyword || !m_useKeywordTypeNames ) {

					sb.Append( type.Namespace );
					sb.Append( "." );
				}

				FormatName( type, sb );
			}
		}

		private void FormatName( Type type, StringBuilder sb ) {

			if( type.IsArray ) {
				FormatName( type.GetElementType(), sb );
				FormatArrayDefinition( type, sb );
			} else {

				if( m_useKeywordTypeNames && HasKeywordTypeName( type ) ) {
					FormatKeywordTypeName( type, sb );

				} else if( type.IsGenericType ) {
					FormatGenericTypeName( type, sb );

				} else {
					sb.Append( type.Name );
				}
			}
		}

		private void FormatGenericTypeName( Type type, StringBuilder sb ) {

			string name = GetGenericTypeDefinitionName( type );
			sb.Append( name );
			sb.Append( "<" );

			Type[] genArgs = type.GetGenericArguments();
			for( int i = 0; i < genArgs.Length; i++ ) {

				if( i > 0 ) {
					sb.Append( "," );
				}

				this.FormatFullName( genArgs[ i ], sb );
			}

			sb.Append( ">" );
		}

		private void FormatKeywordTypeName( Type type, StringBuilder sb ) {

			switch( type.Name ) {

				case ( "Boolean" ):
					sb.Append( "bool" );
					break;

				case ( "Byte" ):
					sb.Append( "byte" );
					break;

				case ( "Char" ):
					sb.Append( "char" );
					break;

				case ( "Decimal" ):
					sb.Append( "decimal" );
					break;

				case ( "Double" ):
					sb.Append( "double" );
					break;

				case ( "Single" ):
					sb.Append( "float" );
					break;

				case ( "Int16" ):
					sb.Append( "short" );
					break;

				case ( "Int32" ):
					sb.Append( "int" );
					break;

				case ( "Int64" ):
					sb.Append( "long" );
					break;

				case ( "SByte" ):
					sb.Append( "sbyte" );
					break;

				case ( "String" ):
					sb.Append( "string" );
					break;

				case ( "UInt16" ):
					sb.Append( "ushort" );
					break;

				case ( "UInt32" ):
					sb.Append( "uint" );
					break;

				case ( "UInt64" ):
					sb.Append( "ulong" );
					break;

				default:
					sb.Append( type.Name );
					break;
			}
		}

		private string GetGenericTypeDefinitionName( Type type ) {

			if( type.IsGenericTypeDefinition ) {

				Match match = m_genTypeDefName.Match( type.Name );
				if( match.Success ) {

					string name = match.Groups[ "name" ].Value;
					return name;

				} else {
					throw new NotSupportedException( "The type's generic type definition name is not supported." );
				}

			} else if( type.IsGenericType ) {
				return GetGenericTypeDefinitionName( type.GetGenericTypeDefinition() );

			} else {
				throw new ArgumentException( "The specified type is not a generic type." );
			}
		}

		private bool HasKeywordTypeName( Type type ) {

			bool hasKeyword = (
					(
						type.IsPrimitive
						&& !type.Equals( typeof( System.IntPtr ) )
						&& !type.Equals( typeof( System.UIntPtr ) )
					)
					|| type.Equals( typeof( string ) )
					|| type.Equals( typeof( decimal ) )
				);

			return hasKeyword;

		}
	}
}
