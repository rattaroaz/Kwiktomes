; ModuleID = 'compressed_assemblies.x86_64.ll'
source_filename = "compressed_assemblies.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 163, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [163 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15664; uint32_t buffer_offset
	}, ; 1: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 31328; uint32_t buffer_offset
	}, ; 2: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 47000; uint32_t buffer_offset
	}, ; 3: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 62664; uint32_t buffer_offset
	}, ; 4: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 78288; uint32_t buffer_offset
	}, ; 5: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 93960; uint32_t buffer_offset
	}, ; 6: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 109624; uint32_t buffer_offset
	}, ; 7: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 125256; uint32_t buffer_offset
	}, ; 8: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 140920; uint32_t buffer_offset
	}, ; 9: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 156552; uint32_t buffer_offset
	}, ; 10: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 172224; uint32_t buffer_offset
	}, ; 11: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 187848; uint32_t buffer_offset
	}, ; 12: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 203520; uint32_t buffer_offset
	}, ; 13: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 219192; uint32_t buffer_offset
	}, ; 14: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 234816; uint32_t buffer_offset
	}, ; 15: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 250480; uint32_t buffer_offset
	}, ; 16: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 266152; uint32_t buffer_offset
	}, ; 17: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 281784; uint32_t buffer_offset
	}, ; 18: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 297408; uint32_t buffer_offset
	}, ; 19: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 313032; uint32_t buffer_offset
	}, ; 20: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 328704; uint32_t buffer_offset
	}, ; 21: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 344368; uint32_t buffer_offset
	}, ; 22: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 360032; uint32_t buffer_offset
	}, ; 23: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 375656; uint32_t buffer_offset
	}, ; 24: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 391288; uint32_t buffer_offset
	}, ; 25: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 406952; uint32_t buffer_offset
	}, ; 26: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 422576; uint32_t buffer_offset
	}, ; 27: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 438248; uint32_t buffer_offset
	}, ; 28: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 453912; uint32_t buffer_offset
	}, ; 29: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 469584; uint32_t buffer_offset
	}, ; 30: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 485256; uint32_t buffer_offset
	}, ; 31: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 500928; uint32_t buffer_offset
	}, ; 32: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 516600; uint32_t buffer_offset
	}, ; 33: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 532224; uint32_t buffer_offset
	}, ; 34: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 253952, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 538368; uint32_t buffer_offset
	}, ; 35: Microsoft.AspNetCore.Components
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 792320; uint32_t buffer_offset
	}, ; 36: Microsoft.AspNetCore.Components.Forms
	%struct.CompressedAssemblyDescriptor {
		i32 100864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 825088; uint32_t buffer_offset
	}, ; 37: Microsoft.AspNetCore.Components.Web
	%struct.CompressedAssemblyDescriptor {
		i32 114448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 925952; uint32_t buffer_offset
	}, ; 38: Microsoft.AspNetCore.Components.WebView
	%struct.CompressedAssemblyDescriptor {
		i32 70448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1040400; uint32_t buffer_offset
	}, ; 39: Microsoft.AspNetCore.Components.WebView.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 88064, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1110848; uint32_t buffer_offset
	}, ; 40: Microsoft.Data.Sqlite
	%struct.CompressedAssemblyDescriptor {
		i32 2683432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1198912; uint32_t buffer_offset
	}, ; 41: Microsoft.EntityFrameworkCore
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3882344; uint32_t buffer_offset
	}, ; 42: Microsoft.EntityFrameworkCore.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 2137136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3897192; uint32_t buffer_offset
	}, ; 43: Microsoft.EntityFrameworkCore.Relational
	%struct.CompressedAssemblyDescriptor {
		i32 294968, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6034328; uint32_t buffer_offset
	}, ; 44: Microsoft.EntityFrameworkCore.Sqlite
	%struct.CompressedAssemblyDescriptor {
		i32 11264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6329296; uint32_t buffer_offset
	}, ; 45: Microsoft.Extensions.Caching.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 26112, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6340560; uint32_t buffer_offset
	}, ; 46: Microsoft.Extensions.Caching.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6366672; uint32_t buffer_offset
	}, ; 47: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6382544; uint32_t buffer_offset
	}, ; 48: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 47104, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6389200; uint32_t buffer_offset
	}, ; 49: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 33792, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6436304; uint32_t buffer_offset
	}, ; 50: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 35840, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6470096; uint32_t buffer_offset
	}, ; 51: Microsoft.Extensions.DependencyModel
	%struct.CompressedAssemblyDescriptor {
		i32 8192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6505936; uint32_t buffer_offset
	}, ; 52: Microsoft.Extensions.Diagnostics.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6514128; uint32_t buffer_offset
	}, ; 53: Microsoft.Extensions.FileProviders.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 7680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6523344; uint32_t buffer_offset
	}, ; 54: Microsoft.Extensions.FileProviders.Composite
	%struct.CompressedAssemblyDescriptor {
		i32 19968, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6531024; uint32_t buffer_offset
	}, ; 55: Microsoft.Extensions.FileProviders.Physical
	%struct.CompressedAssemblyDescriptor {
		i32 27648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6550992; uint32_t buffer_offset
	}, ; 56: Microsoft.Extensions.FileSystemGlobbing
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6578640; uint32_t buffer_offset
	}, ; 57: Microsoft.Extensions.Hosting.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 19968, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6584784; uint32_t buffer_offset
	}, ; 58: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6604752; uint32_t buffer_offset
	}, ; 59: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6643664; uint32_t buffer_offset
	}, ; 60: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6661072; uint32_t buffer_offset
	}, ; 61: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 8704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6674896; uint32_t buffer_offset
	}, ; 62: Microsoft.Extensions.Validation
	%struct.CompressedAssemblyDescriptor {
		i32 44544, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6683600; uint32_t buffer_offset
	}, ; 63: Microsoft.JSInterop
	%struct.CompressedAssemblyDescriptor {
		i32 1925392, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6728144; uint32_t buffer_offset
	}, ; 64: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 133432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8653536; uint32_t buffer_offset
	}, ; 65: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 867128, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8786968; uint32_t buffer_offset
	}, ; 66: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 65024, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9654096; uint32_t buffer_offset
	}, ; 67: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 208648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9719120; uint32_t buffer_offset
	}, ; 68: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 358400, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9927768; uint32_t buffer_offset
	}, ; 69: QuestPDF
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10286168; uint32_t buffer_offset
	}, ; 70: SQLitePCLRaw.batteries_v2
	%struct.CompressedAssemblyDescriptor {
		i32 51200, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10291800; uint32_t buffer_offset
	}, ; 71: SQLitePCLRaw.core
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10343000; uint32_t buffer_offset
	}, ; 72: SQLitePCLRaw.lib.e_sqlite3.android
	%struct.CompressedAssemblyDescriptor {
		i32 36864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10348120; uint32_t buffer_offset
	}, ; 73: SQLitePCLRaw.provider.e_sqlite3
	%struct.CompressedAssemblyDescriptor {
		i32 73216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10384984; uint32_t buffer_offset
	}, ; 74: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 582656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10458200; uint32_t buffer_offset
	}, ; 75: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11040856; uint32_t buffer_offset
	}, ; 76: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11058264; uint32_t buffer_offset
	}, ; 77: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11077208; uint32_t buffer_offset
	}, ; 78: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 78336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11099736; uint32_t buffer_offset
	}, ; 79: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 592384, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11178072; uint32_t buffer_offset
	}, ; 80: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11770456; uint32_t buffer_offset
	}, ; 81: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11797080; uint32_t buffer_offset
	}, ; 82: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 46592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11806808; uint32_t buffer_offset
	}, ; 83: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 233984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11853400; uint32_t buffer_offset
	}, ; 84: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 23552, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12087384; uint32_t buffer_offset
	}, ; 85: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12110936; uint32_t buffer_offset
	}, ; 86: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12129880; uint32_t buffer_offset
	}, ; 87: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12162648; uint32_t buffer_offset
	}, ; 88: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12176472; uint32_t buffer_offset
	}, ; 89: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 92672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12215896; uint32_t buffer_offset
	}, ; 90: Xamarin.AndroidX.Navigation.Common.Android
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12308568; uint32_t buffer_offset
	}, ; 91: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 65024, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12328024; uint32_t buffer_offset
	}, ; 92: Xamarin.AndroidX.Navigation.Runtime.Android
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12393048; uint32_t buffer_offset
	}, ; 93: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 454144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12420184; uint32_t buffer_offset
	}, ; 94: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12874328; uint32_t buffer_offset
	}, ; 95: Xamarin.AndroidX.SavedState.SavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 41472, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12886616; uint32_t buffer_offset
	}, ; 96: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 62464, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12928088; uint32_t buffer_offset
	}, ; 97: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 39936, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12990552; uint32_t buffer_offset
	}, ; 98: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 674304, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13030488; uint32_t buffer_offset
	}, ; 99: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 90624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13704792; uint32_t buffer_offset
	}, ; 100: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 28672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13795416; uint32_t buffer_offset
	}, ; 101: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 91648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13824088; uint32_t buffer_offset
	}, ; 102: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 1216000, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13915736; uint32_t buffer_offset
	}, ; 103: Kwiktomes
	%struct.CompressedAssemblyDescriptor {
		i32 33792, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15131736; uint32_t buffer_offset
	}, ; 104: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 77824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15165528; uint32_t buffer_offset
	}, ; 105: System.Collections.Immutable
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15243352; uint32_t buffer_offset
	}, ; 106: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 12800, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15261272; uint32_t buffer_offset
	}, ; 107: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 65024, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15274072; uint32_t buffer_offset
	}, ; 108: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15339096; uint32_t buffer_offset
	}, ; 109: System.ComponentModel.Annotations
	%struct.CompressedAssemblyDescriptor {
		i32 15360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15370840; uint32_t buffer_offset
	}, ; 110: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 143360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15386200; uint32_t buffer_offset
	}, ; 111: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15529560; uint32_t buffer_offset
	}, ; 112: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15534680; uint32_t buffer_offset
	}, ; 113: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 557056, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15546968; uint32_t buffer_offset
	}, ; 114: System.Data.Common
	%struct.CompressedAssemblyDescriptor {
		i32 52736, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16104024; uint32_t buffer_offset
	}, ; 115: System.Diagnostics.DiagnosticSource
	%struct.CompressedAssemblyDescriptor {
		i32 56320, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16156760; uint32_t buffer_offset
	}, ; 116: System.Diagnostics.Process
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16213080; uint32_t buffer_offset
	}, ; 117: System.Diagnostics.StackTrace
	%struct.CompressedAssemblyDescriptor {
		i32 10240, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16218200; uint32_t buffer_offset
	}, ; 118: System.Diagnostics.TraceSource
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16228440; uint32_t buffer_offset
	}, ; 119: System.Diagnostics.Tracing
	%struct.CompressedAssemblyDescriptor {
		i32 36864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16233560; uint32_t buffer_offset
	}, ; 120: System.Drawing.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16270424; uint32_t buffer_offset
	}, ; 121: System.Drawing
	%struct.CompressedAssemblyDescriptor {
		i32 60416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16275544; uint32_t buffer_offset
	}, ; 122: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16335960; uint32_t buffer_offset
	}, ; 123: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 114176, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16357976; uint32_t buffer_offset
	}, ; 124: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 30720, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16472152; uint32_t buffer_offset
	}, ; 125: System.IO.FileSystem.Watcher
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16502872; uint32_t buffer_offset
	}, ; 126: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16509528; uint32_t buffer_offset
	}, ; 127: System.IO.Pipes
	%struct.CompressedAssemblyDescriptor {
		i32 447488, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16532056; uint32_t buffer_offset
	}, ; 128: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 55808, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 16979544; uint32_t buffer_offset
	}, ; 129: System.Linq.Queryable
	%struct.CompressedAssemblyDescriptor {
		i32 161792, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17035352; uint32_t buffer_offset
	}, ; 130: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17197144; uint32_t buffer_offset
	}, ; 131: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 15360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17214552; uint32_t buffer_offset
	}, ; 132: System.Net.Http.Json
	%struct.CompressedAssemblyDescriptor {
		i32 126464, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17229912; uint32_t buffer_offset
	}, ; 133: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17356376; uint32_t buffer_offset
	}, ; 134: System.Net.NetworkInformation
	%struct.CompressedAssemblyDescriptor {
		i32 66048, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17365592; uint32_t buffer_offset
	}, ; 135: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17431640; uint32_t buffer_offset
	}, ; 136: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 75264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17439320; uint32_t buffer_offset
	}, ; 137: System.Net.Sockets
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17514584; uint32_t buffer_offset
	}, ; 138: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17519704; uint32_t buffer_offset
	}, ; 139: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 74752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17538136; uint32_t buffer_offset
	}, ; 140: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 1350144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 17612888; uint32_t buffer_offset
	}, ; 141: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 18963032; uint32_t buffer_offset
	}, ; 142: System.Runtime.InteropServices.RuntimeInformation
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 18968152; uint32_t buffer_offset
	}, ; 143: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 18977368; uint32_t buffer_offset
	}, ; 144: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 103936, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 18982488; uint32_t buffer_offset
	}, ; 145: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19086424; uint32_t buffer_offset
	}, ; 146: System.Runtime.Serialization.Formatters
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19093592; uint32_t buffer_offset
	}, ; 147: System.Runtime.Serialization.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 16896, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19099224; uint32_t buffer_offset
	}, ; 148: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 124416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19116120; uint32_t buffer_offset
	}, ; 149: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 31232, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19240536; uint32_t buffer_offset
	}, ; 150: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 411648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19271768; uint32_t buffer_offset
	}, ; 151: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 336896, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 19683416; uint32_t buffer_offset
	}, ; 152: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20020312; uint32_t buffer_offset
	}, ; 153: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20025432; uint32_t buffer_offset
	}, ; 154: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 41984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20037720; uint32_t buffer_offset
	}, ; 155: System.Transactions.Local
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20079704; uint32_t buffer_offset
	}, ; 156: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20084824; uint32_t buffer_offset
	}, ; 157: System
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20089944; uint32_t buffer_offset
	}, ; 158: netstandard
	%struct.CompressedAssemblyDescriptor {
		i32 2550784, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 20096600; uint32_t buffer_offset
	}, ; 159: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 171008, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 22647384; uint32_t buffer_offset
	}, ; 160: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 21536, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 22818392; uint32_t buffer_offset
	}, ; 161: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 2038784, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 22839928; uint32_t buffer_offset
	} ; 162: Mono.Android
], align 16

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 24878712, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [24878712 x i8] zeroinitializer, align 16

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
