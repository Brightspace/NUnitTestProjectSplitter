using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NUnitTestProjectSplitter.Helpers {

	public class DebugStopwatch : IDisposable {

		private static readonly IDictionary<string, long> m_stopwatches = new ConcurrentDictionary<string, long>();

		private readonly string m_actionName;
		private readonly Stopwatch m_stopwatch;

		public DebugStopwatch( string actionName ) {
			m_actionName = actionName;
			m_stopwatch = Stopwatch.StartNew();
		}

		public void Dispose() {
			m_stopwatch.Stop();

			if( m_stopwatches.ContainsKey( m_actionName ) ) {
				m_stopwatches[m_actionName] = m_stopwatches[m_actionName] + m_stopwatch.ElapsedMilliseconds;
			} else {
				m_stopwatches[m_actionName] = m_stopwatch.ElapsedMilliseconds;
			}
			
		}

		internal static void Report( IndentedTextWriter writer ) {

			writer.WriteLine( "Elapsed Times:" );
			writer.Indent++;

			int ident = m_stopwatches.Keys.Max( actionName => actionName.Length );
			foreach( var stopWatch in m_stopwatches.OrderBy( x=>x.Key ) ) {
				writer.WriteLine( $"{stopWatch.Key.PadRight( ident )} : {TimeSpan.FromMilliseconds( stopWatch.Value )}" );
			}

			writer.Indent--;

		}
	}
}
