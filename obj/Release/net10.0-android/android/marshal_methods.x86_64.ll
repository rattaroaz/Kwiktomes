; ModuleID = 'marshal_methods.x86_64.ll'
source_filename = "marshal_methods.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [163 x ptr] zeroinitializer, align 16

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [489 x i64] [
	i64 u0x0071cf2d27b7d61e, ; 0: lib_Xamarin.AndroidX.SwipeRefreshLayout.dll.so => 96
	i64 u0x01109b0e4d99e61f, ; 1: System.ComponentModel.Annotations.dll => 109
	i64 u0x02a4c5a44384f885, ; 2: Microsoft.Extensions.Caching.Memory => 46
	i64 u0x02abedc11addc1ed, ; 3: lib_Mono.Android.Runtime.dll.so => 161
	i64 u0x032267b2a94db371, ; 4: lib_Xamarin.AndroidX.AppCompat.dll.so => 75
	i64 u0x0363ac97a4cb84e6, ; 5: SQLitePCLRaw.provider.e_sqlite3.dll => 73
	i64 u0x043032f1d071fae0, ; 6: ru/Microsoft.Maui.Controls.resources => 24
	i64 u0x044440a55165631e, ; 7: lib-cs-Microsoft.Maui.Controls.resources.dll.so => 2
	i64 u0x046eb1581a80c6b0, ; 8: vi/Microsoft.Maui.Controls.resources => 30
	i64 u0x0517ef04e06e9f76, ; 9: System.Net.Primitives => 135
	i64 u0x0565d18c6da3de38, ; 10: Xamarin.AndroidX.RecyclerView => 94
	i64 u0x057bf9fa9fb09f7c, ; 11: Microsoft.Data.Sqlite.dll => 40
	i64 u0x0581db89237110e9, ; 12: lib_System.Collections.dll.so => 108
	i64 u0x05989cb940b225a9, ; 13: Microsoft.Maui.dll => 66
	i64 u0x05ef98b6a1db882c, ; 14: lib_Microsoft.Data.Sqlite.dll.so => 40
	i64 u0x06076b5d2b581f08, ; 15: zh-HK/Microsoft.Maui.Controls.resources => 31
	i64 u0x0680a433c781bb3d, ; 16: Xamarin.AndroidX.Collection.Jvm => 78
	i64 u0x0690533f9fc14683, ; 17: lib_Microsoft.AspNetCore.Components.dll.so => 35
	i64 u0x07c57877c7ba78ad, ; 18: ru/Microsoft.Maui.Controls.resources.dll => 24
	i64 u0x07dcdc7460a0c5e4, ; 19: System.Collections.NonGeneric => 106
	i64 u0x08f3c9788ee2153c, ; 20: Xamarin.AndroidX.DrawerLayout => 83
	i64 u0x09138715c92dba90, ; 21: lib_System.ComponentModel.Annotations.dll.so => 109
	i64 u0x0919c28b89381a0b, ; 22: lib_Microsoft.Extensions.Options.dll.so => 60
	i64 u0x092266563089ae3e, ; 23: lib_System.Collections.NonGeneric.dll.so => 106
	i64 u0x09d144a7e214d457, ; 24: System.Security.Cryptography => 149
	i64 u0x0a805f95d98f597b, ; 25: lib_Microsoft.Extensions.Caching.Abstractions.dll.so => 45
	i64 u0x0b3b632c3bbee20c, ; 26: sk/Microsoft.Maui.Controls.resources => 25
	i64 u0x0b6aff547b84fbe9, ; 27: Xamarin.KotlinX.Serialization.Core.Jvm => 102
	i64 u0x0be2e1f8ce4064ed, ; 28: Xamarin.AndroidX.ViewPager => 97
	i64 u0x0c3ca6cc978e2aae, ; 29: pt-BR/Microsoft.Maui.Controls.resources => 21
	i64 u0x0c59ad9fbbd43abe, ; 30: Mono.Android => 162
	i64 u0x0c7790f60165fc06, ; 31: lib_Microsoft.Maui.Essentials.dll.so => 67
	i64 u0x102a31b45304b1da, ; 32: Xamarin.AndroidX.CustomView => 82
	i64 u0x10f6cfcbcf801616, ; 33: System.IO.Compression.Brotli => 123
	i64 u0x125b7f94acb989db, ; 34: Xamarin.AndroidX.RecyclerView.dll => 94
	i64 u0x13a01de0cbc3f06c, ; 35: lib-fr-Microsoft.Maui.Controls.resources.dll.so => 8
	i64 u0x13f1e5e209e91af4, ; 36: lib_Java.Interop.dll.so => 160
	i64 u0x13f1e880c25d96d1, ; 37: he/Microsoft.Maui.Controls.resources => 9
	i64 u0x143d8ea60a6a4011, ; 38: Microsoft.Extensions.DependencyInjection.Abstractions => 50
	i64 u0x14c2b07f231287a9, ; 39: Kwiktomes => 103
	i64 u0x16054fdcb6b3098b, ; 40: Microsoft.Extensions.DependencyModel.dll => 51
	i64 u0x16bf2a22df043a09, ; 41: System.IO.Pipes.dll => 127
	i64 u0x17125c9a85b4929f, ; 42: lib_netstandard.dll.so => 158
	i64 u0x17b56e25558a5d36, ; 43: lib-hu-Microsoft.Maui.Controls.resources.dll.so => 12
	i64 u0x17f9358913beb16a, ; 44: System.Text.Encodings.Web => 150
	i64 u0x18402a709e357f3b, ; 45: lib_Xamarin.KotlinX.Serialization.Core.Jvm.dll.so => 102
	i64 u0x18f0ce884e87d89a, ; 46: nb/Microsoft.Maui.Controls.resources.dll => 18
	i64 u0x1a91866a319e9259, ; 47: lib_System.Collections.Concurrent.dll.so => 104
	i64 u0x1aac34d1917ba5d3, ; 48: lib_System.dll.so => 157
	i64 u0x1aad60783ffa3e5b, ; 49: lib-th-Microsoft.Maui.Controls.resources.dll.so => 27
	i64 u0x1b8700ce6e547c0b, ; 50: lib_Microsoft.AspNetCore.Components.Forms.dll.so => 36
	i64 u0x1c5217a9e4973753, ; 51: lib_Microsoft.Extensions.FileProviders.Physical.dll.so => 55
	i64 u0x1c753b5ff15bce1b, ; 52: Mono.Android.Runtime.dll => 161
	i64 u0x1dbb0c2c6a999acb, ; 53: System.Diagnostics.StackTrace => 117
	i64 u0x1e3d87657e9659bc, ; 54: Xamarin.AndroidX.Navigation.UI => 93
	i64 u0x1e71143913d56c10, ; 55: lib-ko-Microsoft.Maui.Controls.resources.dll.so => 16
	i64 u0x1ed8fcce5e9b50a0, ; 56: Microsoft.Extensions.Options.dll => 60
	i64 u0x209375905fcc1bad, ; 57: lib_System.IO.Compression.Brotli.dll.so => 123
	i64 u0x20fab3cf2dfbc8df, ; 58: lib_System.Diagnostics.Process.dll.so => 116
	i64 u0x210c5bb16b7260a2, ; 59: lib_Microsoft.Extensions.Validation.dll.so => 62
	i64 u0x2174319c0d835bc9, ; 60: System.Runtime => 148
	i64 u0x220fd4f2e7c48170, ; 61: th/Microsoft.Maui.Controls.resources => 27
	i64 u0x224538d85ed15a82, ; 62: System.IO.Pipes => 127
	i64 u0x237be844f1f812c7, ; 63: System.Threading.Thread.dll => 153
	i64 u0x23807c59646ec4f3, ; 64: lib_Microsoft.EntityFrameworkCore.dll.so => 41
	i64 u0x2407aef2bbe8fadf, ; 65: System.Console => 113
	i64 u0x240abe014b27e7d3, ; 66: Xamarin.AndroidX.Core.dll => 80
	i64 u0x247619fe4413f8bf, ; 67: System.Runtime.Serialization.Primitives.dll => 147
	i64 u0x252073cc3caa62c2, ; 68: fr/Microsoft.Maui.Controls.resources.dll => 8
	i64 u0x25a0a7eff76ea08e, ; 69: SQLitePCLRaw.batteries_v2.dll => 70
	i64 u0x2662c629b96b0b30, ; 70: lib_Xamarin.Kotlin.StdLib.dll.so => 100
	i64 u0x268c1439f13bcc29, ; 71: lib_Microsoft.Extensions.Primitives.dll.so => 61
	i64 u0x273f3515de5faf0d, ; 72: id/Microsoft.Maui.Controls.resources.dll => 13
	i64 u0x2742545f9094896d, ; 73: hr/Microsoft.Maui.Controls.resources => 11
	i64 u0x27458c1163a5ce22, ; 74: Microsoft.Extensions.Validation => 62
	i64 u0x27b410442fad6cf1, ; 75: Java.Interop.dll => 160
	i64 u0x27d02a8c78fe0900, ; 76: QuestPDF.dll => 69
	i64 u0x2801845a2c71fbfb, ; 77: System.Net.Primitives.dll => 135
	i64 u0x28e52865585a1ebe, ; 78: Microsoft.Extensions.Diagnostics.Abstractions.dll => 52
	i64 u0x29aeab763a527e52, ; 79: lib_Xamarin.AndroidX.Navigation.Common.Android.dll.so => 90
	i64 u0x2a128783efe70ba0, ; 80: uk/Microsoft.Maui.Controls.resources.dll => 29
	i64 u0x2a3b095612184159, ; 81: lib_System.Net.NetworkInformation.dll.so => 134
	i64 u0x2a6507a5ffabdf28, ; 82: System.Diagnostics.TraceSource.dll => 118
	i64 u0x2ad156c8e1354139, ; 83: fi/Microsoft.Maui.Controls.resources => 7
	i64 u0x2af298f63581d886, ; 84: System.Text.RegularExpressions.dll => 152
	i64 u0x2afc1c4f898552ee, ; 85: lib_System.Formats.Asn1.dll.so => 122
	i64 u0x2b148910ed40fbf9, ; 86: zh-Hant/Microsoft.Maui.Controls.resources.dll => 33
	i64 u0x2b4d4904cebfa4e9, ; 87: Microsoft.Extensions.FileSystemGlobbing => 56
	i64 u0x2c8bd14bb93a7d82, ; 88: lib-pl-Microsoft.Maui.Controls.resources.dll.so => 20
	i64 u0x2d169d318a968379, ; 89: System.Threading.dll => 154
	i64 u0x2d47774b7d993f59, ; 90: sv/Microsoft.Maui.Controls.resources.dll => 26
	i64 u0x2db915caf23548d2, ; 91: System.Text.Json.dll => 151
	i64 u0x2e6f1f226821322a, ; 92: el/Microsoft.Maui.Controls.resources.dll => 5
	i64 u0x2e8ff3fae87a8245, ; 93: lib_Microsoft.JSInterop.dll.so => 63
	i64 u0x2f02f94df3200fe5, ; 94: System.Diagnostics.Process => 116
	i64 u0x2f2e98e1c89b1aff, ; 95: System.Xml.ReaderWriter => 156
	i64 u0x2f5911d9ba814e4e, ; 96: System.Diagnostics.Tracing => 119
	i64 u0x2feb4d2fcda05cfd, ; 97: Microsoft.Extensions.Caching.Abstractions.dll => 45
	i64 u0x309ee9eeec09a71e, ; 98: lib_Xamarin.AndroidX.Fragment.dll.so => 84
	i64 u0x31195fef5d8fb552, ; 99: _Microsoft.Android.Resource.Designer.dll => 34
	i64 u0x32243413e774362a, ; 100: Xamarin.AndroidX.CardView.dll => 77
	i64 u0x3235427f8d12dae1, ; 101: lib_System.Drawing.Primitives.dll.so => 120
	i64 u0x329753a17a517811, ; 102: fr/Microsoft.Maui.Controls.resources => 8
	i64 u0x32aa989ff07a84ff, ; 103: lib_System.Xml.ReaderWriter.dll.so => 156
	i64 u0x33642d5508314e46, ; 104: Microsoft.Extensions.FileSystemGlobbing.dll => 56
	i64 u0x33829542f112d59b, ; 105: System.Collections.Immutable => 105
	i64 u0x33a31443733849fe, ; 106: lib-es-Microsoft.Maui.Controls.resources.dll.so => 6
	i64 u0x341abc357fbb4ebf, ; 107: lib_System.Net.Sockets.dll.so => 137
	i64 u0x34bd01fd4be06ee3, ; 108: lib_Microsoft.Extensions.FileProviders.Composite.dll.so => 54
	i64 u0x34dfd74fe2afcf37, ; 109: Microsoft.Maui => 66
	i64 u0x34e292762d9615df, ; 110: cs/Microsoft.Maui.Controls.resources.dll => 2
	i64 u0x3508234247f48404, ; 111: Microsoft.Maui.Controls => 64
	i64 u0x353590da528c9d22, ; 112: System.ComponentModel.Annotations => 109
	i64 u0x3549870798b4cd30, ; 113: lib_Xamarin.AndroidX.ViewPager2.dll.so => 98
	i64 u0x355282fc1c909694, ; 114: Microsoft.Extensions.Configuration => 47
	i64 u0x380134e03b1e160a, ; 115: System.Collections.Immutable.dll => 105
	i64 u0x385c17636bb6fe6e, ; 116: Xamarin.AndroidX.CustomView.dll => 82
	i64 u0x393c226616977fdb, ; 117: lib_Xamarin.AndroidX.ViewPager.dll.so => 97
	i64 u0x395e37c3334cf82a, ; 118: lib-ca-Microsoft.Maui.Controls.resources.dll.so => 1
	i64 u0x39c3107c28752af1, ; 119: lib_Microsoft.Extensions.FileProviders.Abstractions.dll.so => 53
	i64 u0x3be6248c2bc7dc8c, ; 120: Microsoft.JSInterop.dll => 63
	i64 u0x3be99b43dd39dd37, ; 121: Xamarin.AndroidX.SavedState.SavedState.Android => 95
	i64 u0x3c7c495f58ac5ee9, ; 122: Xamarin.Kotlin.StdLib => 100
	i64 u0x3d9c2a242b040a50, ; 123: lib_Xamarin.AndroidX.Core.dll.so => 80
	i64 u0x3da7781d6333a8fe, ; 124: SQLitePCLRaw.batteries_v2 => 70
	i64 u0x3e7f8912b96e5065, ; 125: Microsoft.AspNetCore.Components.WebView.dll => 38
	i64 u0x3f6f5914291cdcf7, ; 126: Microsoft.Extensions.Hosting.Abstractions => 57
	i64 u0x400eb4a58d8d746b, ; 127: lib_QuestPDF.dll.so => 69
	i64 u0x41cab042be111c34, ; 128: lib_Xamarin.AndroidX.AppCompat.AppCompatResources.dll.so => 76
	i64 u0x43375950ec7c1b6a, ; 129: netstandard.dll => 158
	i64 u0x434c4e1d9284cdae, ; 130: Mono.Android.dll => 162
	i64 u0x43950f84de7cc79a, ; 131: pl/Microsoft.Maui.Controls.resources.dll => 20
	i64 u0x4499fa3c8e494654, ; 132: lib_System.Runtime.Serialization.Primitives.dll.so => 147
	i64 u0x4515080865a951a5, ; 133: Xamarin.Kotlin.StdLib.dll => 100
	i64 u0x453c1277f85cf368, ; 134: lib_Microsoft.EntityFrameworkCore.Abstractions.dll.so => 42
	i64 u0x45c40276a42e283e, ; 135: System.Diagnostics.TraceSource => 118
	i64 u0x45fcc9fd66f25095, ; 136: Microsoft.Extensions.DependencyModel => 51
	i64 u0x46a4213bc97fe5ae, ; 137: lib-ru-Microsoft.Maui.Controls.resources.dll.so => 24
	i64 u0x47daf4e1afbada10, ; 138: pt/Microsoft.Maui.Controls.resources => 22
	i64 u0x49e952f19a4e2022, ; 139: System.ObjectModel => 139
	i64 u0x4a5667b2462a664b, ; 140: lib_Xamarin.AndroidX.Navigation.UI.dll.so => 93
	i64 u0x4b7b6532ded934b7, ; 141: System.Text.Json => 151
	i64 u0x4c2029a97af23a8d, ; 142: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android => 88
	i64 u0x4c7755cf07ad2d5f, ; 143: System.Net.Http.Json.dll => 132
	i64 u0x4ca014ceac582c86, ; 144: Microsoft.EntityFrameworkCore.Relational.dll => 43
	i64 u0x4cc5f15266470798, ; 145: lib_Xamarin.AndroidX.Loader.dll.so => 89
	i64 u0x4cf6f67dc77aacd2, ; 146: System.Net.NetworkInformation.dll => 134
	i64 u0x4d479f968a05e504, ; 147: System.Linq.Expressions.dll => 128
	i64 u0x4d55a010ffc4faff, ; 148: System.Private.Xml => 141
	i64 u0x4d95fccc1f67c7ca, ; 149: System.Runtime.Loader.dll => 144
	i64 u0x4dcf44c3c9b076a2, ; 150: it/Microsoft.Maui.Controls.resources.dll => 14
	i64 u0x4dd9247f1d2c3235, ; 151: Xamarin.AndroidX.Loader.dll => 89
	i64 u0x4df510084e2a0bae, ; 152: Microsoft.JSInterop => 63
	i64 u0x4e32f00cb0937401, ; 153: Mono.Android.Runtime => 161
	i64 u0x4f21ee6ef9eb527e, ; 154: ca/Microsoft.Maui.Controls.resources => 1
	i64 u0x4fd5f3ee53d0a4f0, ; 155: SQLitePCLRaw.lib.e_sqlite3.android => 72
	i64 u0x5037f0be3c28c7a3, ; 156: lib_Microsoft.Maui.Controls.dll.so => 64
	i64 u0x5131bbe80989093f, ; 157: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 87
	i64 u0x51bb8a2afe774e32, ; 158: System.Drawing => 121
	i64 u0x526ce79eb8e90527, ; 159: lib_System.Net.Primitives.dll.so => 135
	i64 u0x52829f00b4467c38, ; 160: lib_System.Data.Common.dll.so => 114
	i64 u0x529ffe06f39ab8db, ; 161: Xamarin.AndroidX.Core => 80
	i64 u0x52ff996554dbf352, ; 162: Microsoft.Maui.Graphics => 68
	i64 u0x535f7e40e8fef8af, ; 163: lib-sk-Microsoft.Maui.Controls.resources.dll.so => 25
	i64 u0x53a96d5c86c9e194, ; 164: System.Net.NetworkInformation => 134
	i64 u0x53be1038a61e8d44, ; 165: System.Runtime.InteropServices.RuntimeInformation.dll => 142
	i64 u0x53c3014b9437e684, ; 166: lib-zh-HK-Microsoft.Maui.Controls.resources.dll.so => 31
	i64 u0x54795225dd1587af, ; 167: lib_System.Runtime.dll.so => 148
	i64 u0x54b851bc9b470503, ; 168: Xamarin.AndroidX.Navigation.Common.Android => 90
	i64 u0x556e8b63b660ab8b, ; 169: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 85
	i64 u0x5588627c9a108ec9, ; 170: System.Collections.Specialized => 107
	i64 u0x571c5cfbec5ae8e2, ; 171: System.Private.Uri => 140
	i64 u0x578cd35c91d7b347, ; 172: lib_SQLitePCLRaw.core.dll.so => 71
	i64 u0x579a06fed6eec900, ; 173: System.Private.CoreLib.dll => 159
	i64 u0x57adda3c951abb33, ; 174: Microsoft.Extensions.Hosting.Abstractions.dll => 57
	i64 u0x57c542c14049b66d, ; 175: System.Diagnostics.DiagnosticSource => 115
	i64 u0x58601b2dda4a27b9, ; 176: lib-ja-Microsoft.Maui.Controls.resources.dll.so => 15
	i64 u0x58688d9af496b168, ; 177: Microsoft.Extensions.DependencyInjection.dll => 49
	i64 u0x5a89a886ae30258d, ; 178: lib_Xamarin.AndroidX.CoordinatorLayout.dll.so => 79
	i64 u0x5a8f6699f4a1caa9, ; 179: lib_System.Threading.dll.so => 154
	i64 u0x5ae9cd33b15841bf, ; 180: System.ComponentModel => 112
	i64 u0x5b5f0e240a06a2a2, ; 181: da/Microsoft.Maui.Controls.resources.dll => 3
	i64 u0x5c393624b8176517, ; 182: lib_Microsoft.Extensions.Logging.dll.so => 58
	i64 u0x5d25ef991dd9a85c, ; 183: Microsoft.AspNetCore.Components.WebView.Maui.dll => 39
	i64 u0x5db0cbbd1028510e, ; 184: lib_System.Runtime.InteropServices.dll.so => 143
	i64 u0x5db30905d3e5013b, ; 185: Xamarin.AndroidX.Collection.Jvm.dll => 78
	i64 u0x5e467bc8f09ad026, ; 186: System.Collections.Specialized.dll => 107
	i64 u0x5ea92fdb19ec8c4c, ; 187: System.Text.Encodings.Web.dll => 150
	i64 u0x5eb8046dd40e9ac3, ; 188: System.ComponentModel.Primitives => 110
	i64 u0x5f36ccf5c6a57e24, ; 189: System.Xml.ReaderWriter.dll => 156
	i64 u0x5f4294b9b63cb842, ; 190: System.Data.Common => 114
	i64 u0x5f7399e166075632, ; 191: lib_SQLitePCLRaw.lib.e_sqlite3.android.dll.so => 72
	i64 u0x5f9a2d823f664957, ; 192: lib-el-Microsoft.Maui.Controls.resources.dll.so => 5
	i64 u0x609f4b7b63d802d4, ; 193: lib_Microsoft.Extensions.DependencyInjection.dll.so => 49
	i64 u0x60cd4e33d7e60134, ; 194: Xamarin.KotlinX.Coroutines.Core.Jvm => 101
	i64 u0x60f62d786afcf130, ; 195: System.Memory => 131
	i64 u0x61be8d1299194243, ; 196: Microsoft.Maui.Controls.Xaml => 65
	i64 u0x61d2cba29557038f, ; 197: de/Microsoft.Maui.Controls.resources => 4
	i64 u0x61d88f399afb2f45, ; 198: lib_System.Runtime.Loader.dll.so => 144
	i64 u0x622eef6f9e59068d, ; 199: System.Private.CoreLib => 159
	i64 u0x639fb99a7bef11de, ; 200: Xamarin.AndroidX.Navigation.Runtime.Android.dll => 92
	i64 u0x63f1f6883c1e23c2, ; 201: lib_System.Collections.Immutable.dll.so => 105
	i64 u0x6400f68068c1e9f1, ; 202: Xamarin.Google.Android.Material.dll => 99
	i64 u0x65ecac39144dd3cc, ; 203: Microsoft.Maui.Controls.dll => 64
	i64 u0x65ece51227bfa724, ; 204: lib_System.Runtime.Numerics.dll.so => 145
	i64 u0x6692e924eade1b29, ; 205: lib_System.Console.dll.so => 113
	i64 u0x66a4e5c6a3fb0bae, ; 206: lib_Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll.so => 87
	i64 u0x66d13304ce1a3efa, ; 207: Xamarin.AndroidX.CursorAdapter => 81
	i64 u0x68558ec653afa616, ; 208: lib-da-Microsoft.Maui.Controls.resources.dll.so => 3
	i64 u0x6872ec7a2e36b1ac, ; 209: System.Drawing.Primitives.dll => 120
	i64 u0x68fbbbe2eb455198, ; 210: System.Formats.Asn1 => 122
	i64 u0x69063fc0ba8e6bdd, ; 211: he/Microsoft.Maui.Controls.resources.dll => 9
	i64 u0x699dffb2427a2d71, ; 212: SQLitePCLRaw.lib.e_sqlite3.android.dll => 72
	i64 u0x6a4d7577b2317255, ; 213: System.Runtime.InteropServices.dll => 143
	i64 u0x6ace3b74b15ee4a4, ; 214: nb/Microsoft.Maui.Controls.resources => 18
	i64 u0x6d12bfaa99c72b1f, ; 215: lib_Microsoft.Maui.Graphics.dll.so => 68
	i64 u0x6d79993361e10ef2, ; 216: Microsoft.Extensions.Primitives => 61
	i64 u0x6d86d56b84c8eb71, ; 217: lib_Xamarin.AndroidX.CursorAdapter.dll.so => 81
	i64 u0x6d9bea6b3e895cf7, ; 218: Microsoft.Extensions.Primitives.dll => 61
	i64 u0x6e25a02c3833319a, ; 219: lib_Xamarin.AndroidX.Navigation.Fragment.dll.so => 91
	i64 u0x6fd2265da78b93a4, ; 220: lib_Microsoft.Maui.dll.so => 66
	i64 u0x6fdfc7de82c33008, ; 221: cs/Microsoft.Maui.Controls.resources => 2
	i64 u0x6ffc4967cc47ba57, ; 222: System.IO.FileSystem.Watcher.dll => 125
	i64 u0x70d67fb826a6921f, ; 223: Microsoft.Extensions.Validation.dll => 62
	i64 u0x70e99f48c05cb921, ; 224: tr/Microsoft.Maui.Controls.resources.dll => 28
	i64 u0x70fd3deda22442d2, ; 225: lib-nb-Microsoft.Maui.Controls.resources.dll.so => 18
	i64 u0x717530326f808838, ; 226: lib_Microsoft.Extensions.Diagnostics.Abstractions.dll.so => 52
	i64 u0x71a495ea3761dde8, ; 227: lib-it-Microsoft.Maui.Controls.resources.dll.so => 14
	i64 u0x71ad672adbe48f35, ; 228: System.ComponentModel.Primitives.dll => 110
	i64 u0x72b1fb4109e08d7b, ; 229: lib-hr-Microsoft.Maui.Controls.resources.dll.so => 11
	i64 u0x73e4ce94e2eb6ffc, ; 230: lib_System.Memory.dll.so => 131
	i64 u0x73f2645914262879, ; 231: lib_Microsoft.EntityFrameworkCore.Sqlite.dll.so => 44
	i64 u0x755a91767330b3d4, ; 232: lib_Microsoft.Extensions.Configuration.dll.so => 47
	i64 u0x76ca07b878f44da0, ; 233: System.Runtime.Numerics.dll => 145
	i64 u0x780bc73597a503a9, ; 234: lib-ms-Microsoft.Maui.Controls.resources.dll.so => 17
	i64 u0x783606d1e53e7a1a, ; 235: th/Microsoft.Maui.Controls.resources.dll => 27
	i64 u0x78a45e51311409b6, ; 236: Xamarin.AndroidX.Fragment.dll => 84
	i64 u0x7a71889545dcdb00, ; 237: lib_Microsoft.AspNetCore.Components.WebView.dll.so => 38
	i64 u0x7adb8da2ac89b647, ; 238: fi/Microsoft.Maui.Controls.resources.dll => 7
	i64 u0x7b150145c0a9058c, ; 239: Microsoft.Data.Sqlite => 40
	i64 u0x7bef86a4335c4870, ; 240: System.ComponentModel.TypeConverter => 111
	i64 u0x7c0820144cd34d6a, ; 241: sk/Microsoft.Maui.Controls.resources.dll => 25
	i64 u0x7c2a0bd1e0f988fc, ; 242: lib-de-Microsoft.Maui.Controls.resources.dll.so => 4
	i64 u0x7c60acf6404e96b6, ; 243: Xamarin.AndroidX.Navigation.Common.Android.dll => 90
	i64 u0x7d649b75d580bb42, ; 244: ms/Microsoft.Maui.Controls.resources.dll => 17
	i64 u0x7d8b5821548f89e7, ; 245: Microsoft.AspNetCore.Components.Forms => 36
	i64 u0x7d8ee2bdc8e3aad1, ; 246: System.Numerics.Vectors => 138
	i64 u0x7dfc3d6d9d8d7b70, ; 247: System.Collections => 108
	i64 u0x7e2e564fa2f76c65, ; 248: lib_System.Diagnostics.Tracing.dll.so => 119
	i64 u0x7e946809d6008ef2, ; 249: lib_System.ObjectModel.dll.so => 139
	i64 u0x7ecc13347c8fd849, ; 250: lib_System.ComponentModel.dll.so => 112
	i64 u0x7f00ddd9b9ca5a13, ; 251: Xamarin.AndroidX.ViewPager.dll => 97
	i64 u0x7f9351cd44b1273f, ; 252: Microsoft.Extensions.Configuration.Abstractions => 48
	i64 u0x7fbd557c99b3ce6f, ; 253: lib_Xamarin.AndroidX.Lifecycle.LiveData.Core.dll.so => 86
	i64 u0x80fa55b6d1b0be99, ; 254: SQLitePCLRaw.provider.e_sqlite3 => 73
	i64 u0x8101a73bd4533440, ; 255: Microsoft.AspNetCore.Components.Web => 37
	i64 u0x812c069d5cdecc17, ; 256: System.dll => 157
	i64 u0x81ab745f6c0f5ce6, ; 257: zh-Hant/Microsoft.Maui.Controls.resources => 33
	i64 u0x8277f2be6b5ce05f, ; 258: Xamarin.AndroidX.AppCompat => 75
	i64 u0x828f06563b30bc50, ; 259: lib_Xamarin.AndroidX.CardView.dll.so => 77
	i64 u0x82df8f5532a10c59, ; 260: lib_System.Drawing.dll.so => 121
	i64 u0x82f6403342e12049, ; 261: uk/Microsoft.Maui.Controls.resources => 29
	i64 u0x83c14ba66c8e2b8c, ; 262: zh-Hans/Microsoft.Maui.Controls.resources => 32
	i64 u0x83de69860da6cbdd, ; 263: Microsoft.Extensions.FileProviders.Composite => 54
	i64 u0x84ae73148a4557d2, ; 264: lib_System.IO.Pipes.dll.so => 127
	i64 u0x84cd5cdec0f54bcc, ; 265: lib_Microsoft.EntityFrameworkCore.Relational.dll.so => 43
	i64 u0x86a909228dc7657b, ; 266: lib-zh-Hant-Microsoft.Maui.Controls.resources.dll.so => 33
	i64 u0x86b3e00c36b84509, ; 267: Microsoft.Extensions.Configuration.dll => 47
	i64 u0x8704193f462e892e, ; 268: lib_Microsoft.Extensions.FileSystemGlobbing.dll.so => 56
	i64 u0x87c4b8a492b176ad, ; 269: Microsoft.EntityFrameworkCore.Abstractions => 42
	i64 u0x87c69b87d9283884, ; 270: lib_System.Threading.Thread.dll.so => 153
	i64 u0x87f6569b25707834, ; 271: System.IO.Compression.Brotli.dll => 123
	i64 u0x8842b3a5d2d3fb36, ; 272: Microsoft.Maui.Essentials => 67
	i64 u0x88bda98e0cffb7a9, ; 273: lib_Xamarin.KotlinX.Coroutines.Core.Jvm.dll.so => 101
	i64 u0x8930322c7bd8f768, ; 274: netstandard => 158
	i64 u0x897a606c9e39c75f, ; 275: lib_System.ComponentModel.Primitives.dll.so => 110
	i64 u0x898a5c6bc9e47ec1, ; 276: lib_Xamarin.AndroidX.SavedState.SavedState.Android.dll.so => 95
	i64 u0x89c5188089ec2cd5, ; 277: lib_System.Runtime.InteropServices.RuntimeInformation.dll.so => 142
	i64 u0x8a399a706fcbce4b, ; 278: Microsoft.Extensions.Caching.Abstractions => 45
	i64 u0x8ad229ea26432ee2, ; 279: Xamarin.AndroidX.Loader => 89
	i64 u0x8b4ff5d0fdd5faa1, ; 280: lib_System.Diagnostics.DiagnosticSource.dll.so => 115
	i64 u0x8b8d01333a96d0b5, ; 281: System.Diagnostics.Process.dll => 116
	i64 u0x8b9ceca7acae3451, ; 282: lib-he-Microsoft.Maui.Controls.resources.dll.so => 9
	i64 u0x8c575135aa1ccef4, ; 283: Microsoft.Extensions.FileProviders.Abstractions => 53
	i64 u0x8d0f420977c2c1c7, ; 284: Xamarin.AndroidX.CursorAdapter.dll => 81
	i64 u0x8d52a25632e81824, ; 285: Microsoft.EntityFrameworkCore.Sqlite.dll => 44
	i64 u0x8d7b8ab4b3310ead, ; 286: System.Threading => 154
	i64 u0x8da188285aadfe8e, ; 287: System.Collections.Concurrent => 104
	i64 u0x8ee08b8194a30f48, ; 288: lib-hi-Microsoft.Maui.Controls.resources.dll.so => 10
	i64 u0x8ef7601039857a44, ; 289: lib-ro-Microsoft.Maui.Controls.resources.dll.so => 23
	i64 u0x8ef9414937d93a0a, ; 290: SQLitePCLRaw.core.dll => 71
	i64 u0x8f32c6f611f6ffab, ; 291: pt/Microsoft.Maui.Controls.resources.dll => 22
	i64 u0x8f8829d21c8985a4, ; 292: lib-pt-BR-Microsoft.Maui.Controls.resources.dll.so => 21
	i64 u0x8fd27d934d7b3a55, ; 293: SQLitePCLRaw.core => 71
	i64 u0x90263f8448b8f572, ; 294: lib_System.Diagnostics.TraceSource.dll.so => 118
	i64 u0x903101b46fb73a04, ; 295: _Microsoft.Android.Resource.Designer => 34
	i64 u0x90393bd4865292f3, ; 296: lib_System.IO.Compression.dll.so => 124
	i64 u0x90634f86c5ebe2b5, ; 297: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 87
	i64 u0x907b636704ad79ef, ; 298: lib_Microsoft.Maui.Controls.Xaml.dll.so => 65
	i64 u0x91418dc638b29e68, ; 299: lib_Xamarin.AndroidX.CustomView.dll.so => 82
	i64 u0x9157bd523cd7ed36, ; 300: lib_System.Text.Json.dll.so => 151
	i64 u0x91a74f07b30d37e2, ; 301: System.Linq.dll => 130
	i64 u0x91fa41a87223399f, ; 302: ca/Microsoft.Maui.Controls.resources.dll => 1
	i64 u0x93cfa73ab28d6e35, ; 303: ms/Microsoft.Maui.Controls.resources => 17
	i64 u0x944077d8ca3c6580, ; 304: System.IO.Compression.dll => 124
	i64 u0x967fc325e09bfa8c, ; 305: es/Microsoft.Maui.Controls.resources => 6
	i64 u0x96dede8e816b5a25, ; 306: Kwiktomes.dll => 103
	i64 u0x9732d8dbddea3d9a, ; 307: id/Microsoft.Maui.Controls.resources => 13
	i64 u0x978be80e5210d31b, ; 308: Microsoft.Maui.Graphics.dll => 68
	i64 u0x97b8c771ea3e4220, ; 309: System.ComponentModel.dll => 112
	i64 u0x97e144c9d3c6976e, ; 310: System.Collections.Concurrent.dll => 104
	i64 u0x98b05cc81e6f333c, ; 311: Xamarin.AndroidX.SavedState.SavedState.Android.dll => 95
	i64 u0x991d510397f92d9d, ; 312: System.Linq.Expressions => 128
	i64 u0x99cdc6d1f2d3a72f, ; 313: ko/Microsoft.Maui.Controls.resources.dll => 16
	i64 u0x9b211a749105beac, ; 314: System.Transactions.Local => 155
	i64 u0x9d5dbcf5a48583fe, ; 315: lib_Xamarin.AndroidX.Activity.dll.so => 74
	i64 u0x9d74dee1a7725f34, ; 316: Microsoft.Extensions.Configuration.Abstractions.dll => 48
	i64 u0x9dd0e195825d65c6, ; 317: lib_Xamarin.AndroidX.Navigation.Runtime.Android.dll.so => 92
	i64 u0x9e4534b6adaf6e84, ; 318: nl/Microsoft.Maui.Controls.resources => 19
	i64 u0x9ef542cf1f78c506, ; 319: Xamarin.AndroidX.Lifecycle.LiveData.Core => 86
	i64 u0x9fbb2961ca18e5c2, ; 320: Microsoft.Extensions.FileProviders.Physical.dll => 55
	i64 u0xa0d8259f4cc284ec, ; 321: lib_System.Security.Cryptography.dll.so => 149
	i64 u0xa1440773ee9d341e, ; 322: Xamarin.Google.Android.Material => 99
	i64 u0xa1b9d7c27f47219f, ; 323: Xamarin.AndroidX.Navigation.UI.dll => 93
	i64 u0xa2572680829d2c7c, ; 324: System.IO.Pipelines.dll => 126
	i64 u0xa35eeea065361708, ; 325: QuestPDF => 69
	i64 u0xa46aa1eaa214539b, ; 326: ko/Microsoft.Maui.Controls.resources => 16
	i64 u0xa4e62983cf1e3674, ; 327: Microsoft.AspNetCore.Components.Forms.dll => 36
	i64 u0xa4edc8f2ceae241a, ; 328: System.Data.Common.dll => 114
	i64 u0xa5494f40f128ce6a, ; 329: System.Runtime.Serialization.Formatters.dll => 146
	i64 u0xa5b7152421ed6d98, ; 330: lib_System.IO.FileSystem.Watcher.dll.so => 125
	i64 u0xa5e599d1e0524750, ; 331: System.Numerics.Vectors.dll => 138
	i64 u0xa5f1ba49b85dd355, ; 332: System.Security.Cryptography.dll => 149
	i64 u0xa68a420042bb9b1f, ; 333: Xamarin.AndroidX.DrawerLayout.dll => 83
	i64 u0xa78ce3745383236a, ; 334: Xamarin.AndroidX.Lifecycle.Common.Jvm => 85
	i64 u0xa7c31b56b4dc7b33, ; 335: hu/Microsoft.Maui.Controls.resources => 12
	i64 u0xa82fd211eef00a5b, ; 336: Microsoft.Extensions.FileProviders.Physical => 55
	i64 u0xaa2219c8e3449ff5, ; 337: Microsoft.Extensions.Logging.Abstractions => 59
	i64 u0xaa443ac34067eeef, ; 338: System.Private.Xml.dll => 141
	i64 u0xaa52de307ef5d1dd, ; 339: System.Net.Http => 133
	i64 u0xaa9a7b0214a5cc5c, ; 340: System.Diagnostics.StackTrace.dll => 117
	i64 u0xaaaf86367285a918, ; 341: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 50
	i64 u0xaaf84bb3f052a265, ; 342: el/Microsoft.Maui.Controls.resources => 5
	i64 u0xab9c1b2687d86b0b, ; 343: lib_System.Linq.Expressions.dll.so => 128
	i64 u0xac2af3fa195a15ce, ; 344: System.Runtime.Numerics => 145
	i64 u0xac5376a2a538dc10, ; 345: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 86
	i64 u0xacd46e002c3ccb97, ; 346: ro/Microsoft.Maui.Controls.resources => 23
	i64 u0xad89c07347f1bad6, ; 347: nl/Microsoft.Maui.Controls.resources.dll => 19
	i64 u0xadc90ab061a9e6e4, ; 348: System.ComponentModel.TypeConverter.dll => 111
	i64 u0xae282bcd03739de7, ; 349: Java.Interop => 160
	i64 u0xae53579c90db1107, ; 350: System.ObjectModel.dll => 139
	i64 u0xaf12fb8133ac3fbb, ; 351: Microsoft.EntityFrameworkCore.Sqlite => 44
	i64 u0xb05cc42cd94c6d9d, ; 352: lib-sv-Microsoft.Maui.Controls.resources.dll.so => 26
	i64 u0xb0bb43dc52ea59f9, ; 353: System.Diagnostics.Tracing.dll => 119
	i64 u0xb1ccbf6243328d1c, ; 354: Microsoft.AspNetCore.Components => 35
	i64 u0xb220631954820169, ; 355: System.Text.RegularExpressions => 152
	i64 u0xb2a3f67f3bf29fce, ; 356: da/Microsoft.Maui.Controls.resources => 3
	i64 u0xb3f0a0fcda8d3ebc, ; 357: Xamarin.AndroidX.CardView => 77
	i64 u0xb46be1aa6d4fff93, ; 358: hi/Microsoft.Maui.Controls.resources => 10
	i64 u0xb477491be13109d8, ; 359: ar/Microsoft.Maui.Controls.resources => 0
	i64 u0xb4bd7015ecee9d86, ; 360: System.IO.Pipelines => 126
	i64 u0xb5c7fcdafbc67ee4, ; 361: Microsoft.Extensions.Logging.Abstractions.dll => 59
	i64 u0xb7212c4683a94afe, ; 362: System.Drawing.Primitives => 120
	i64 u0xb7b7753d1f319409, ; 363: sv/Microsoft.Maui.Controls.resources => 26
	i64 u0xb81a2c6e0aee50fe, ; 364: lib_System.Private.CoreLib.dll.so => 159
	i64 u0xb960d6b2200ba320, ; 365: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android.dll => 88
	i64 u0xb9f64d3b230def68, ; 366: lib-pt-Microsoft.Maui.Controls.resources.dll.so => 22
	i64 u0xb9fc3c8a556e3691, ; 367: ja/Microsoft.Maui.Controls.resources => 15
	i64 u0xba48785529705af9, ; 368: System.Collections.dll => 108
	i64 u0xbaf762c4825c14e9, ; 369: Microsoft.AspNetCore.Components.WebView => 38
	i64 u0xbb65706fde942ce3, ; 370: System.Net.Sockets => 137
	i64 u0xbbd180354b67271a, ; 371: System.Runtime.Serialization.Formatters => 146
	i64 u0xbc22a245dab70cb4, ; 372: lib_SQLitePCLRaw.provider.e_sqlite3.dll.so => 73
	i64 u0xbd0e2c0d55246576, ; 373: System.Net.Http.dll => 133
	i64 u0xbd437a2cdb333d0d, ; 374: Xamarin.AndroidX.ViewPager2 => 98
	i64 u0xbee38d4a88835966, ; 375: Xamarin.AndroidX.AppCompat.AppCompatResources => 76
	i64 u0xbfc1e1fb3095f2b3, ; 376: lib_System.Net.Http.Json.dll.so => 132
	i64 u0xc040a4ab55817f58, ; 377: ar/Microsoft.Maui.Controls.resources.dll => 0
	i64 u0xc0d928351ab5ca77, ; 378: System.Console.dll => 113
	i64 u0xc12b8b3afa48329c, ; 379: lib_System.Linq.dll.so => 130
	i64 u0xc1c2cb7af77b8858, ; 380: Microsoft.EntityFrameworkCore => 41
	i64 u0xc1ff9ae3cdb6e1e6, ; 381: Xamarin.AndroidX.Activity.dll => 74
	i64 u0xc28c50f32f81cc73, ; 382: ja/Microsoft.Maui.Controls.resources.dll => 15
	i64 u0xc2a3bca55b573141, ; 383: System.IO.FileSystem.Watcher => 125
	i64 u0xc2bcfec99f69365e, ; 384: Xamarin.AndroidX.ViewPager2.dll => 98
	i64 u0xc3492f8f90f96ce4, ; 385: lib_Microsoft.Extensions.DependencyModel.dll.so => 51
	i64 u0xc472ce300460ccb6, ; 386: Microsoft.EntityFrameworkCore.dll => 41
	i64 u0xc4d69851fe06342f, ; 387: lib_Microsoft.Extensions.Caching.Memory.dll.so => 46
	i64 u0xc50fded0ded1418c, ; 388: lib_System.ComponentModel.TypeConverter.dll.so => 111
	i64 u0xc519125d6bc8fb11, ; 389: lib_System.Net.Requests.dll.so => 136
	i64 u0xc5293b19e4dc230e, ; 390: Xamarin.AndroidX.Navigation.Fragment => 91
	i64 u0xc5325b2fcb37446f, ; 391: lib_System.Private.Xml.dll.so => 141
	i64 u0xc5a0f4b95a699af7, ; 392: lib_System.Private.Uri.dll.so => 140
	i64 u0xc74d70d4aa96cef3, ; 393: Xamarin.AndroidX.Navigation.Runtime.Android => 92
	i64 u0xc858a28d9ee5a6c5, ; 394: lib_System.Collections.Specialized.dll.so => 107
	i64 u0xca3110fea81c8916, ; 395: Microsoft.AspNetCore.Components.Web.dll => 37
	i64 u0xca32340d8d54dcd5, ; 396: Microsoft.Extensions.Caching.Memory.dll => 46
	i64 u0xca3a723e7342c5b6, ; 397: lib-tr-Microsoft.Maui.Controls.resources.dll.so => 28
	i64 u0xcab3493c70141c2d, ; 398: pl/Microsoft.Maui.Controls.resources => 20
	i64 u0xcacfddc9f7c6de76, ; 399: ro/Microsoft.Maui.Controls.resources.dll => 23
	i64 u0xcb45618372c47127, ; 400: Microsoft.EntityFrameworkCore.Relational => 43
	i64 u0xcbd4fdd9cef4a294, ; 401: lib__Microsoft.Android.Resource.Designer.dll.so => 34
	i64 u0xcc2876b32ef2794c, ; 402: lib_System.Text.RegularExpressions.dll.so => 152
	i64 u0xcc5c3bb714c4561e, ; 403: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 101
	i64 u0xcc76886e09b88260, ; 404: Xamarin.KotlinX.Serialization.Core.Jvm.dll => 102
	i64 u0xccf25c4b634ccd3a, ; 405: zh-Hans/Microsoft.Maui.Controls.resources.dll => 32
	i64 u0xcd10a42808629144, ; 406: System.Net.Requests => 136
	i64 u0xcdd0c48b6937b21c, ; 407: Xamarin.AndroidX.SwipeRefreshLayout => 96
	i64 u0xcf23d8093f3ceadf, ; 408: System.Diagnostics.DiagnosticSource.dll => 115
	i64 u0xd1194e1d8a8de83c, ; 409: lib_Xamarin.AndroidX.Lifecycle.Common.Jvm.dll.so => 85
	i64 u0xd16fd7fb9bbcd43e, ; 410: Microsoft.Extensions.Diagnostics.Abstractions => 52
	i64 u0xd2505d8abeed6983, ; 411: lib_Microsoft.AspNetCore.Components.Web.dll.so => 37
	i64 u0xd333d0af9e423810, ; 412: System.Runtime.InteropServices => 143
	i64 u0xd3426d966bb704f5, ; 413: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 76
	i64 u0xd3651b6fc3125825, ; 414: System.Private.Uri.dll => 140
	i64 u0xd373685349b1fe8b, ; 415: Microsoft.Extensions.Logging.dll => 58
	i64 u0xd3e4c8d6a2d5d470, ; 416: it/Microsoft.Maui.Controls.resources => 14
	i64 u0xd42655883bb8c19f, ; 417: Microsoft.EntityFrameworkCore.Abstractions.dll => 42
	i64 u0xd4645626dffec99d, ; 418: lib_Microsoft.Extensions.DependencyInjection.Abstractions.dll.so => 50
	i64 u0xd46b4a8758d1f3ee, ; 419: Microsoft.Extensions.FileProviders.Composite.dll => 54
	i64 u0xd6d21782156bc35b, ; 420: Xamarin.AndroidX.SwipeRefreshLayout.dll => 96
	i64 u0xd72329819cbbbc44, ; 421: lib_Microsoft.Extensions.Configuration.Abstractions.dll.so => 48
	i64 u0xd7b3764ada9d341d, ; 422: lib_Microsoft.Extensions.Logging.Abstractions.dll.so => 59
	i64 u0xda1dfa4c534a9251, ; 423: Microsoft.Extensions.DependencyInjection => 49
	i64 u0xdad05a11827959a3, ; 424: System.Collections.NonGeneric.dll => 106
	i64 u0xdb5383ab5865c007, ; 425: lib-vi-Microsoft.Maui.Controls.resources.dll.so => 30
	i64 u0xdbeda89f832aa805, ; 426: vi/Microsoft.Maui.Controls.resources.dll => 30
	i64 u0xdbf2a779fbc3ac31, ; 427: System.Transactions.Local.dll => 155
	i64 u0xdbf9607a441b4505, ; 428: System.Linq => 130
	i64 u0xdc75032002d1a212, ; 429: lib_System.Transactions.Local.dll.so => 155
	i64 u0xdca8be7403f92d4f, ; 430: lib_System.Linq.Queryable.dll.so => 129
	i64 u0xdce2c53525640bf3, ; 431: Microsoft.Extensions.Logging => 58
	i64 u0xdd2b722d78ef5f43, ; 432: System.Runtime.dll => 148
	i64 u0xdd67031857c72f96, ; 433: lib_System.Text.Encodings.Web.dll.so => 150
	i64 u0xdde30e6b77aa6f6c, ; 434: lib-zh-Hans-Microsoft.Maui.Controls.resources.dll.so => 32
	i64 u0xde8769ebda7d8647, ; 435: hr/Microsoft.Maui.Controls.resources.dll => 11
	i64 u0xe0142572c095a480, ; 436: Xamarin.AndroidX.AppCompat.dll => 75
	i64 u0xe02f89350ec78051, ; 437: Xamarin.AndroidX.CoordinatorLayout.dll => 79
	i64 u0xe192a588d4410686, ; 438: lib_System.IO.Pipelines.dll.so => 126
	i64 u0xe1a08bd3fa539e0d, ; 439: System.Runtime.Loader => 144
	i64 u0xe24095a7afddaab3, ; 440: lib_Microsoft.Extensions.Hosting.Abstractions.dll.so => 57
	i64 u0xe2420585aeceb728, ; 441: System.Net.Requests.dll => 136
	i64 u0xe29b73bc11392966, ; 442: lib-id-Microsoft.Maui.Controls.resources.dll.so => 13
	i64 u0xe31089e70e4e84ee, ; 443: Microsoft.AspNetCore.Components.WebView.Maui => 39
	i64 u0xe3811d68d4fe8463, ; 444: pt-BR/Microsoft.Maui.Controls.resources.dll => 21
	i64 u0xe494f7ced4ecd10a, ; 445: hu/Microsoft.Maui.Controls.resources.dll => 12
	i64 u0xe4a9b1e40d1e8917, ; 446: lib-fi-Microsoft.Maui.Controls.resources.dll.so => 7
	i64 u0xe4f74a0b5bf9703f, ; 447: System.Runtime.Serialization.Primitives => 147
	i64 u0xe5434e8a119ceb69, ; 448: lib_Mono.Android.dll.so => 162
	i64 u0xe7e03cc18dcdeb49, ; 449: lib_System.Diagnostics.StackTrace.dll.so => 117
	i64 u0xe89a2a9ef110899b, ; 450: System.Drawing.dll => 121
	i64 u0xe9772100456fb4b4, ; 451: Microsoft.AspNetCore.Components.dll => 35
	i64 u0xedc4817167106c23, ; 452: System.Net.Sockets.dll => 137
	i64 u0xedc632067fb20ff3, ; 453: System.Memory.dll => 131
	i64 u0xeeb7ebb80150501b, ; 454: lib_Xamarin.AndroidX.Collection.Jvm.dll.so => 78
	i64 u0xef72742e1bcca27a, ; 455: Microsoft.Maui.Essentials.dll => 67
	i64 u0xefec0b7fdc57ec42, ; 456: Xamarin.AndroidX.Activity => 74
	i64 u0xf00c29406ea45e19, ; 457: es/Microsoft.Maui.Controls.resources.dll => 6
	i64 u0xf11b621fc87b983f, ; 458: Microsoft.Maui.Controls.Xaml.dll => 65
	i64 u0xf1c4b4005493d871, ; 459: System.Formats.Asn1.dll => 122
	i64 u0xf22514cfad2d598b, ; 460: lib_Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android.dll.so => 88
	i64 u0xf238bd79489d3a96, ; 461: lib-nl-Microsoft.Maui.Controls.resources.dll.so => 19
	i64 u0xf37221fda4ef8830, ; 462: lib_Xamarin.Google.Android.Material.dll.so => 99
	i64 u0xf3ddfe05336abf29, ; 463: System => 157
	i64 u0xf4103170a1de5bd0, ; 464: System.Linq.Queryable.dll => 129
	i64 u0xf4c1dd70a5496a17, ; 465: System.IO.Compression => 124
	i64 u0xf6077741019d7428, ; 466: Xamarin.AndroidX.CoordinatorLayout => 79
	i64 u0xf77b20923f07c667, ; 467: de/Microsoft.Maui.Controls.resources.dll => 4
	i64 u0xf7adefdaa79affa7, ; 468: lib_Kwiktomes.dll.so => 103
	i64 u0xf7e2cac4c45067b3, ; 469: lib_System.Numerics.Vectors.dll.so => 138
	i64 u0xf7e74930e0e3d214, ; 470: zh-HK/Microsoft.Maui.Controls.resources.dll => 31
	i64 u0xf84773b5c81e3cef, ; 471: lib-uk-Microsoft.Maui.Controls.resources.dll.so => 29
	i64 u0xf8aac5ea82de1348, ; 472: System.Linq.Queryable => 129
	i64 u0xf8e045dc345b2ea3, ; 473: lib_Xamarin.AndroidX.RecyclerView.dll.so => 94
	i64 u0xf96c777a2a0686f4, ; 474: hi/Microsoft.Maui.Controls.resources.dll => 10
	i64 u0xf9eec5bb3a6aedc6, ; 475: Microsoft.Extensions.Options => 60
	i64 u0xfa504dfa0f097d72, ; 476: Microsoft.Extensions.FileProviders.Abstractions.dll => 53
	i64 u0xfa5ed7226d978949, ; 477: lib-ar-Microsoft.Maui.Controls.resources.dll.so => 0
	i64 u0xfa645d91e9fc4cba, ; 478: System.Threading.Thread => 153
	i64 u0xfb022853d73b7fa5, ; 479: lib_SQLitePCLRaw.batteries_v2.dll.so => 70
	i64 u0xfbf0a31c9fc34bc4, ; 480: lib_System.Net.Http.dll.so => 133
	i64 u0xfc6b7527cc280b3f, ; 481: lib_System.Runtime.Serialization.Formatters.dll.so => 146
	i64 u0xfc719aec26adf9d9, ; 482: Xamarin.AndroidX.Navigation.Fragment.dll => 91
	i64 u0xfd22f00870e40ae0, ; 483: lib_Xamarin.AndroidX.DrawerLayout.dll.so => 83
	i64 u0xfd2e866c678cac90, ; 484: lib_Microsoft.AspNetCore.Components.WebView.Maui.dll.so => 39
	i64 u0xfd49b3c1a76e2748, ; 485: System.Runtime.InteropServices.RuntimeInformation => 142
	i64 u0xfd583f7657b6a1cb, ; 486: Xamarin.AndroidX.Fragment => 84
	i64 u0xfeae9952cf03b8cb, ; 487: tr/Microsoft.Maui.Controls.resources => 28
	i64 u0xff9b54613e0d2cc8 ; 488: System.Net.Http.Json => 132
], align 16

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [489 x i32] [
	i32 96, i32 109, i32 46, i32 161, i32 75, i32 73, i32 24, i32 2,
	i32 30, i32 135, i32 94, i32 40, i32 108, i32 66, i32 40, i32 31,
	i32 78, i32 35, i32 24, i32 106, i32 83, i32 109, i32 60, i32 106,
	i32 149, i32 45, i32 25, i32 102, i32 97, i32 21, i32 162, i32 67,
	i32 82, i32 123, i32 94, i32 8, i32 160, i32 9, i32 50, i32 103,
	i32 51, i32 127, i32 158, i32 12, i32 150, i32 102, i32 18, i32 104,
	i32 157, i32 27, i32 36, i32 55, i32 161, i32 117, i32 93, i32 16,
	i32 60, i32 123, i32 116, i32 62, i32 148, i32 27, i32 127, i32 153,
	i32 41, i32 113, i32 80, i32 147, i32 8, i32 70, i32 100, i32 61,
	i32 13, i32 11, i32 62, i32 160, i32 69, i32 135, i32 52, i32 90,
	i32 29, i32 134, i32 118, i32 7, i32 152, i32 122, i32 33, i32 56,
	i32 20, i32 154, i32 26, i32 151, i32 5, i32 63, i32 116, i32 156,
	i32 119, i32 45, i32 84, i32 34, i32 77, i32 120, i32 8, i32 156,
	i32 56, i32 105, i32 6, i32 137, i32 54, i32 66, i32 2, i32 64,
	i32 109, i32 98, i32 47, i32 105, i32 82, i32 97, i32 1, i32 53,
	i32 63, i32 95, i32 100, i32 80, i32 70, i32 38, i32 57, i32 69,
	i32 76, i32 158, i32 162, i32 20, i32 147, i32 100, i32 42, i32 118,
	i32 51, i32 24, i32 22, i32 139, i32 93, i32 151, i32 88, i32 132,
	i32 43, i32 89, i32 134, i32 128, i32 141, i32 144, i32 14, i32 89,
	i32 63, i32 161, i32 1, i32 72, i32 64, i32 87, i32 121, i32 135,
	i32 114, i32 80, i32 68, i32 25, i32 134, i32 142, i32 31, i32 148,
	i32 90, i32 85, i32 107, i32 140, i32 71, i32 159, i32 57, i32 115,
	i32 15, i32 49, i32 79, i32 154, i32 112, i32 3, i32 58, i32 39,
	i32 143, i32 78, i32 107, i32 150, i32 110, i32 156, i32 114, i32 72,
	i32 5, i32 49, i32 101, i32 131, i32 65, i32 4, i32 144, i32 159,
	i32 92, i32 105, i32 99, i32 64, i32 145, i32 113, i32 87, i32 81,
	i32 3, i32 120, i32 122, i32 9, i32 72, i32 143, i32 18, i32 68,
	i32 61, i32 81, i32 61, i32 91, i32 66, i32 2, i32 125, i32 62,
	i32 28, i32 18, i32 52, i32 14, i32 110, i32 11, i32 131, i32 44,
	i32 47, i32 145, i32 17, i32 27, i32 84, i32 38, i32 7, i32 40,
	i32 111, i32 25, i32 4, i32 90, i32 17, i32 36, i32 138, i32 108,
	i32 119, i32 139, i32 112, i32 97, i32 48, i32 86, i32 73, i32 37,
	i32 157, i32 33, i32 75, i32 77, i32 121, i32 29, i32 32, i32 54,
	i32 127, i32 43, i32 33, i32 47, i32 56, i32 42, i32 153, i32 123,
	i32 67, i32 101, i32 158, i32 110, i32 95, i32 142, i32 45, i32 89,
	i32 115, i32 116, i32 9, i32 53, i32 81, i32 44, i32 154, i32 104,
	i32 10, i32 23, i32 71, i32 22, i32 21, i32 71, i32 118, i32 34,
	i32 124, i32 87, i32 65, i32 82, i32 151, i32 130, i32 1, i32 17,
	i32 124, i32 6, i32 103, i32 13, i32 68, i32 112, i32 104, i32 95,
	i32 128, i32 16, i32 155, i32 74, i32 48, i32 92, i32 19, i32 86,
	i32 55, i32 149, i32 99, i32 93, i32 126, i32 69, i32 16, i32 36,
	i32 114, i32 146, i32 125, i32 138, i32 149, i32 83, i32 85, i32 12,
	i32 55, i32 59, i32 141, i32 133, i32 117, i32 50, i32 5, i32 128,
	i32 145, i32 86, i32 23, i32 19, i32 111, i32 160, i32 139, i32 44,
	i32 26, i32 119, i32 35, i32 152, i32 3, i32 77, i32 10, i32 0,
	i32 126, i32 59, i32 120, i32 26, i32 159, i32 88, i32 22, i32 15,
	i32 108, i32 38, i32 137, i32 146, i32 73, i32 133, i32 98, i32 76,
	i32 132, i32 0, i32 113, i32 130, i32 41, i32 74, i32 15, i32 125,
	i32 98, i32 51, i32 41, i32 46, i32 111, i32 136, i32 91, i32 141,
	i32 140, i32 92, i32 107, i32 37, i32 46, i32 28, i32 20, i32 23,
	i32 43, i32 34, i32 152, i32 101, i32 102, i32 32, i32 136, i32 96,
	i32 115, i32 85, i32 52, i32 37, i32 143, i32 76, i32 140, i32 58,
	i32 14, i32 42, i32 50, i32 54, i32 96, i32 48, i32 59, i32 49,
	i32 106, i32 30, i32 30, i32 155, i32 130, i32 155, i32 129, i32 58,
	i32 148, i32 150, i32 32, i32 11, i32 75, i32 79, i32 126, i32 144,
	i32 57, i32 136, i32 13, i32 39, i32 21, i32 12, i32 7, i32 147,
	i32 162, i32 117, i32 121, i32 35, i32 137, i32 131, i32 78, i32 67,
	i32 74, i32 6, i32 65, i32 122, i32 88, i32 19, i32 99, i32 157,
	i32 129, i32 124, i32 79, i32 4, i32 103, i32 138, i32 31, i32 29,
	i32 129, i32 94, i32 10, i32 60, i32 53, i32 0, i32 153, i32 70,
	i32 133, i32 146, i32 91, i32 83, i32 39, i32 142, i32 84, i32 28,
	i32 132
], align 16

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 u0x0000000000000000, ; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.mm.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.mm.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 16

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ 01024bb616e7b80417a2c6d320885bfdb956f20a"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
