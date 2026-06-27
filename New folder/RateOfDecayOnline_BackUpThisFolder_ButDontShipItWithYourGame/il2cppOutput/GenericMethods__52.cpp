#include "pch-cpp.hpp"





template <typename T1, typename T2>
struct InvokerActionInvoker2;
template <typename T1, typename T2>
struct InvokerActionInvoker2<T1, T2*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1 p1, T2* p2)
	{
		void* params[2] = { &p1, p2 };
		method->invoker_method(methodPtr, method, obj, params, params[1]);
	}
};
template <typename T1, typename T2, typename T3>
struct InvokerActionInvoker3;
template <typename T1, typename T2, typename T3>
struct InvokerActionInvoker3<T1*, T2, T3*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2 p2, T3* p3)
	{
		void* params[3] = { p1, &p2, p3 };
		method->invoker_method(methodPtr, method, obj, params, params[2]);
	}
};
template <typename R, typename T1, typename T2>
struct InvokerFuncInvoker2;
template <typename R, typename T1, typename T2>
struct InvokerFuncInvoker2<R, T1, T2*>
{
	static inline R Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1 p1, T2* p2)
	{
		R ret;
		void* params[2] = { &p1, p2 };
		method->invoker_method(methodPtr, method, obj, params, &ret);
		return ret;
	}
};
template <typename R, typename T1, typename T2, typename T3>
struct InvokerFuncInvoker3;
template <typename R, typename T1, typename T2, typename T3>
struct InvokerFuncInvoker3<R, T1*, T2, T3*>
{
	static inline R Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2 p2, T3* p3)
	{
		R ret;
		void* params[3] = { p1, &p2, p3 };
		method->invoker_method(methodPtr, method, obj, params, &ret);
		return ret;
	}
};
template <typename R>
struct ConstrainedFuncInvoker0
{
	static inline R Invoke (RuntimeClass* type, const RuntimeMethod* constrainedMethod, void* boxBuffer, void* obj)
	{
		R ret;
		il2cpp_codegen_runtime_constrained_call(type, constrainedMethod, boxBuffer, obj, NULL, &ret);
		return ret;
	}
};
template <typename R, typename T1>
struct ConstrainedFuncInvoker1;
template <typename R, typename T1>
struct ConstrainedFuncInvoker1<R, T1*>
{
	static inline R Invoke (RuntimeClass* type, const RuntimeMethod* constrainedMethod, void* boxBuffer, void* obj, T1* p1)
	{
		R ret;
		void* params[1] = { p1 };
		il2cpp_codegen_runtime_constrained_call(type, constrainedMethod, boxBuffer, obj, params, &ret);
		return ret;
	}
};

struct Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F;
struct List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73;
struct List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD;
struct List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4;
struct List_1U5BU5D_t37294D7C303231F2FD83B3C398AED0937F4F3206;
struct List_1U5BU5D_tE510DA387DA867AC92F8274325B178A7DE9A209E;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA;
struct __Il2CppFullySharedGenericStructTypeU5BU5D_tF3B929B6E80D0A8C109178E11CE8FF9957B014C1;
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C;
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3;
struct CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7;
struct ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233;
struct CullingAllocationInfo_tB260F5CD0B290F74E145EB16E54B901CC68D9D5A;
struct DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E;
struct EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8;
struct GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1;
struct IAsyncResult_t7B9B5A0ECB35DCEC31B8A8122C37D687369253B5;
struct MethodInfo_t;
struct RenderGraphPass_tEFB5BD685D417024760D82991EEEA4F4D0454A93;
struct String_t;
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700;
struct UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
typedef Il2CppFullySharedGenericStruct Il2CppFullySharedGenericStruct;
struct Baselib_NetworkAddress_HostnameLookupHandle_t254C6E9BD97DBB2771BBF6A2F8500DADA2FCE7A8;
struct Info_tDBEB127ABB26184014A541C0CAD1FC8D1B95DE84;

IL2CPP_EXTERN_C RuntimeClass* Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C const RuntimeMethod* Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_RuntimeMethod_var;
struct Delegate_t_marshaled_com;
struct Delegate_t_marshaled_pinvoke;


IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4  : public RuntimeObject
{
	__Il2CppFullySharedGenericStructTypeU5BU5D_tF3B929B6E80D0A8C109178E11CE8FF9957B014C1* ____items;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06  : public RuntimeObject
{
	CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* ___m_WrappedCommandBuffer;
	RenderGraphPass_tEFB5BD685D417024760D82991EEEA4F4D0454A93* ___m_ExecutingPass;
};
struct Unsafe_t013486CBD5A88F5F394651AB34F2AC5AE97E71E4  : public RuntimeObject
{
};
struct Unsafe_t7A5BFA4CCC4DE54D6A25FB6312C3DB95A35D2B9E  : public RuntimeObject
{
};
struct UnsafeListExtensions_t9F27C0DDB39954D821115ACDC913DC77BCAC720A  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839 
{
	Il2CppFullySharedGenericStruct* ___Ptr;
	int32_t ___Length;
};
struct ReadOnly_t620C834D6C54D781225D753876F232E09821956C 
{
	Il2CppFullySharedGenericStruct* ___Ptr;
	int32_t ___Length;
};
struct AttachmentIndexArray_tDC550BA2CD14AFB3B5545B02331C26903ADA90B5 
{
	int32_t ___a0;
	int32_t ___a1;
	int32_t ___a2;
	int32_t ___a3;
	int32_t ___a4;
	int32_t ___a5;
	int32_t ___a6;
	int32_t ___a7;
	int32_t ___activeAttachments;
};
struct BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 
{
	uint32_t ___value;
};
struct BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C 
{
	uint32_t ___value;
};
struct BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 
{
	uint32_t ___value;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	uint8_t ___m_value;
};
struct Color_tD001788D726C3A7F1379BEED0260B9591F440C1F 
{
	float ___r;
	float ___g;
	float ___b;
	float ___a;
};
struct Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___rgba;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___rgba_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___r;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___r_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___g_OffsetPadding[1];
			uint8_t ___g;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___g_OffsetPadding_forAlignmentOnly[1];
			uint8_t ___g_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___b_OffsetPadding[2];
			uint8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___b_OffsetPadding_forAlignmentOnly[2];
			uint8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___a_OffsetPadding[3];
			uint8_t ___a;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___a_OffsetPadding_forAlignmentOnly[3];
			uint8_t ___a_forAlignmentOnly;
		};
	};
};
struct ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB 
{
	int32_t ___Id;
	int32_t ___Version;
};
struct DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D 
{
	uint64_t ____dateData;
};
struct DepthState_t798415D2C1D9202E555FEE5D4C5FDF6B3A077255 
{
	uint8_t ___m_WriteEnabled;
	int8_t ___m_CompareFunction;
};
struct EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 
{
	union
	{
		struct
		{
			int32_t ___m_Data;
		};
		uint8_t EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8__padding[4];
	};
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
#pragma pack(push, tp, 1)
struct FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					uint8_t ___byte0000;
				};
				#pragma pack(pop, tp)
				struct
				{
					uint8_t ___byte0000_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0001_OffsetPadding[1];
					uint8_t ___byte0001;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0001_OffsetPadding_forAlignmentOnly[1];
					uint8_t ___byte0001_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0002_OffsetPadding[2];
					uint8_t ___byte0002;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0002_OffsetPadding_forAlignmentOnly[2];
					uint8_t ___byte0002_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0003_OffsetPadding[3];
					uint8_t ___byte0003;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0003_OffsetPadding_forAlignmentOnly[3];
					uint8_t ___byte0003_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0004_OffsetPadding[4];
					uint8_t ___byte0004;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0004_OffsetPadding_forAlignmentOnly[4];
					uint8_t ___byte0004_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0005_OffsetPadding[5];
					uint8_t ___byte0005;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0005_OffsetPadding_forAlignmentOnly[5];
					uint8_t ___byte0005_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0006_OffsetPadding[6];
					uint8_t ___byte0006;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0006_OffsetPadding_forAlignmentOnly[6];
					uint8_t ___byte0006_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0007_OffsetPadding[7];
					uint8_t ___byte0007;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0007_OffsetPadding_forAlignmentOnly[7];
					uint8_t ___byte0007_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0008_OffsetPadding[8];
					uint8_t ___byte0008;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0008_OffsetPadding_forAlignmentOnly[8];
					uint8_t ___byte0008_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0009_OffsetPadding[9];
					uint8_t ___byte0009;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0009_OffsetPadding_forAlignmentOnly[9];
					uint8_t ___byte0009_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0010_OffsetPadding[10];
					uint8_t ___byte0010;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0010_OffsetPadding_forAlignmentOnly[10];
					uint8_t ___byte0010_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0011_OffsetPadding[11];
					uint8_t ___byte0011;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0011_OffsetPadding_forAlignmentOnly[11];
					uint8_t ___byte0011_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0012_OffsetPadding[12];
					uint8_t ___byte0012;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0012_OffsetPadding_forAlignmentOnly[12];
					uint8_t ___byte0012_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0013_OffsetPadding[13];
					uint8_t ___byte0013;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0013_OffsetPadding_forAlignmentOnly[13];
					uint8_t ___byte0013_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0014_OffsetPadding[14];
					uint8_t ___byte0014;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0014_OffsetPadding_forAlignmentOnly[14];
					uint8_t ___byte0014_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0015_OffsetPadding[15];
					uint8_t ___byte0015;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0015_OffsetPadding_forAlignmentOnly[15];
					uint8_t ___byte0015_forAlignmentOnly;
				};
			};
		};
		uint8_t FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0__padding[16];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					uint64_t ___byte0000;
				};
				#pragma pack(pop, tp)
				struct
				{
					uint64_t ___byte0000_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0008_OffsetPadding[8];
					uint64_t ___byte0008;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0008_OffsetPadding_forAlignmentOnly[8];
					uint64_t ___byte0008_forAlignmentOnly;
				};
			};
		};
		uint8_t FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4__padding[16];
	};
};
#pragma pack(pop, tp)
struct GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 
{
	uint32_t ___data;
};
struct GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 
{
	int32_t ___U3CindexU3Ek__BackingField;
};
struct GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 
{
	int32_t ___index;
};
struct InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B 
{
	int32_t ___U3CindexU3Ek__BackingField;
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct Int64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3 
{
	int64_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct JobHandle_t5DF5F99902FED3C801A81C05205CEA6CE039EF08 
{
	uint64_t ___jobGroup;
	int32_t ___version;
};
struct Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 
{
	int64_t ___f0;
	int64_t ___f1;
	int64_t ___f2;
	int64_t ___f3;
	int64_t ___f4;
	int64_t ___f5;
	int64_t ___f6;
	int64_t ___f7;
};
struct Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 
{
	float ___m00;
	float ___m10;
	float ___m20;
	float ___m30;
	float ___m01;
	float ___m11;
	float ___m21;
	float ___m31;
	float ___m02;
	float ___m12;
	float ___m22;
	float ___m32;
	float ___m03;
	float ___m13;
	float ___m23;
	float ___m33;
};
struct NetworkPipeline_t83BB00B4CE9811A2B75D37E50EFB85FD60044A69 
{
	int32_t ___Id;
};
struct PhysicsBody_tDCDAC059BCC8FF8A057AD9B683E04545D3B191A7 
{
	int32_t ___m_Index1;
	uint16_t ___m_World0;
	uint16_t ___m_Generation;
};
struct PhysicsJoint_tD7FB5BFA5B2CF4F6A267F59B0038EE32EA89CE9D 
{
	int32_t ___index1;
	uint16_t ___world0;
	uint16_t ___generation;
};
struct PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B 
{
	int32_t ___m_Index1;
	uint16_t ___m_World0;
	uint16_t ___m_Generation;
};
struct Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
};
struct ReadCommand_t5DB46BD58D686FDDFBD8AB7600B9CF676DC7D97F 
{
	void* ___Buffer;
	int64_t ___Offset;
	int64_t ___Size;
};
struct Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D 
{
	float ___m_XMin;
	float ___m_YMin;
	float ___m_Width;
	float ___m_Height;
};
struct RenderQueueRange_t7518252AA6426B1EA45D3D9B394F304EEF784D71 
{
	int32_t ___m_LowerBound;
	int32_t ___m_UpperBound;
};
struct RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 
{
	uint8_t ___m_WriteMask;
	uint8_t ___m_SourceColorBlendMode;
	uint8_t ___m_DestinationColorBlendMode;
	uint8_t ___m_SourceAlphaBlendMode;
	uint8_t ___m_DestinationAlphaBlendMode;
	uint8_t ___m_ColorBlendOperation;
	uint8_t ___m_AlphaBlendOperation;
	uint8_t ___m_Padding;
};
struct ResourceReaderData_t1B57A9C4CC76875899745E115AA53FF40C6412EC 
{
	int32_t ___passId;
	int32_t ___inputSlot;
};
struct ResourceVersionedData_tC935A106FCF6C0800974D2C98DBE14E19A19B1DC 
{
	bool ___written;
	int32_t ___writePassId;
	int32_t ___numReaders;
};
struct ResourceVersionedData_tC935A106FCF6C0800974D2C98DBE14E19A19B1DC_marshaled_pinvoke
{
	int32_t ___written;
	int32_t ___writePassId;
	int32_t ___numReaders;
};
struct ResourceVersionedData_tC935A106FCF6C0800974D2C98DBE14E19A19B1DC_marshaled_com
{
	int32_t ___written;
	int32_t ___writePassId;
	int32_t ___numReaders;
};
struct ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0 
{
	int32_t ___m_Id;
};
struct SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 
{
	int32_t ___U3CindexU3Ek__BackingField;
};
struct Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C 
{
	float ___m_value;
};
struct SortingLayerRange_t96D04CFB4E8824978FEB2CFFFCFEAC37E56D52C9 
{
	int16_t ___m_LowerBound;
	int16_t ___m_UpperBound;
};
struct SphericalHarmonicsL2_tCBFB646455D2069E738976E5B745C6DF023B6BA2 
{
	float ___shr0;
	float ___shr1;
	float ___shr2;
	float ___shr3;
	float ___shr4;
	float ___shr5;
	float ___shr6;
	float ___shr7;
	float ___shr8;
	float ___shg0;
	float ___shg1;
	float ___shg2;
	float ___shg3;
	float ___shg4;
	float ___shg5;
	float ___shg6;
	float ___shg7;
	float ___shg8;
	float ___shb0;
	float ___shb1;
	float ___shb2;
	float ___shb3;
	float ___shb4;
	float ___shb5;
	float ___shb6;
	float ___shb7;
	float ___shb8;
};
struct StencilState_tBE5F7C1134E50C5E93B45A626D4FB4690F1C91A9 
{
	uint8_t ___m_Enabled;
	uint8_t ___m_ReadMask;
	uint8_t ___m_WriteMask;
	uint8_t ___m_Padding;
	uint8_t ___m_CompareFunctionFront;
	uint8_t ___m_PassOperationFront;
	uint8_t ___m_FailOperationFront;
	uint8_t ___m_ZFailOperationFront;
	uint8_t ___m_CompareFunctionBack;
	uint8_t ___m_PassOperationBack;
	uint8_t ___m_FailOperationBack;
	uint8_t ___m_ZFailOperationBack;
};
struct SubviewOcclusionTest_t4C10094E5EF2C745723FEFE4E5749FBB75CAA026 
{
	int32_t ___cullingSplitIndex;
	int32_t ___occluderSubviewIndex;
};
struct UInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455 
{
	uint16_t ___m_value;
};
struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	uint32_t ___m_value;
};
struct UInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF 
{
	uint64_t ___m_value;
};
struct UIntPtr_t 
{
	void* ____pointer;
};
struct UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2  : public BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06
{
};
struct UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t* ___values;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t* ___values_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___keys_OffsetPadding[8];
			uint8_t* ___keys;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___keys_OffsetPadding_forAlignmentOnly[8];
			uint8_t* ___keys_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___next_OffsetPadding[16];
			uint8_t* ___next;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___next_OffsetPadding_forAlignmentOnly[16];
			uint8_t* ___next_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___buckets_OffsetPadding[24];
			uint8_t* ___buckets;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___buckets_OffsetPadding_forAlignmentOnly[24];
			uint8_t* ___buckets_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___keyCapacity_OffsetPadding[32];
			int32_t ___keyCapacity;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___keyCapacity_OffsetPadding_forAlignmentOnly[32];
			int32_t ___keyCapacity_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___bucketCapacityMask_OffsetPadding[36];
			int32_t ___bucketCapacityMask;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___bucketCapacityMask_OffsetPadding_forAlignmentOnly[36];
			int32_t ___bucketCapacityMask_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___allocatedIndexLength_OffsetPadding[40];
			int32_t ___allocatedIndexLength;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___allocatedIndexLength_OffsetPadding_forAlignmentOnly[40];
			int32_t ___allocatedIndexLength_forAlignmentOnly;
		};
	};
};
struct Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 
{
	float ___x;
	float ___y;
};
struct Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A 
{
	int32_t ___m_X;
	int32_t ___m_Y;
};
struct Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 
{
	float ___x;
	float ___y;
	float ___z;
};
struct Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 
{
	int32_t ___m_X;
	int32_t ___m_Y;
	int32_t ___m_Z;
};
struct Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA 
{
	float ___x;
	float ___y;
};
struct float3_t4AB5D88249ADB24F69FFD0793E8ED25E1CC3745E 
{
	float ___x;
	float ___y;
	float ___z;
};
struct float4_t89D9A294E7A79BD81BFBDD18654508532958555E 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
};
struct AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 
{
	uint16_t ___Index;
	uint16_t ___Version;
};
struct Sample_t3BE5915021631BD79173E66EC224867714256407 
{
	int64_t ___Time;
	uint64_t ___Bytes;
};
struct Baselib_NetworkAddress_t2F4AF92B4EEFE31182BADF512CA004AFF48128E0 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___data0;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___data0_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data1_OffsetPadding[1];
			uint8_t ___data1;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data1_OffsetPadding_forAlignmentOnly[1];
			uint8_t ___data1_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data2_OffsetPadding[2];
			uint8_t ___data2;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data2_OffsetPadding_forAlignmentOnly[2];
			uint8_t ___data2_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data3_OffsetPadding[3];
			uint8_t ___data3;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data3_OffsetPadding_forAlignmentOnly[3];
			uint8_t ___data3_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data4_OffsetPadding[4];
			uint8_t ___data4;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data4_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___data4_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data5_OffsetPadding[5];
			uint8_t ___data5;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data5_OffsetPadding_forAlignmentOnly[5];
			uint8_t ___data5_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data6_OffsetPadding[6];
			uint8_t ___data6;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data6_OffsetPadding_forAlignmentOnly[6];
			uint8_t ___data6_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data7_OffsetPadding[7];
			uint8_t ___data7;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data7_OffsetPadding_forAlignmentOnly[7];
			uint8_t ___data7_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data8_OffsetPadding[8];
			uint8_t ___data8;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data8_OffsetPadding_forAlignmentOnly[8];
			uint8_t ___data8_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data9_OffsetPadding[9];
			uint8_t ___data9;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data9_OffsetPadding_forAlignmentOnly[9];
			uint8_t ___data9_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data10_OffsetPadding[10];
			uint8_t ___data10;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data10_OffsetPadding_forAlignmentOnly[10];
			uint8_t ___data10_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data11_OffsetPadding[11];
			uint8_t ___data11;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data11_OffsetPadding_forAlignmentOnly[11];
			uint8_t ___data11_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data12_OffsetPadding[12];
			uint8_t ___data12;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data12_OffsetPadding_forAlignmentOnly[12];
			uint8_t ___data12_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data13_OffsetPadding[13];
			uint8_t ___data13;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data13_OffsetPadding_forAlignmentOnly[13];
			uint8_t ___data13_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data14_OffsetPadding[14];
			uint8_t ___data14;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data14_OffsetPadding_forAlignmentOnly[14];
			uint8_t ___data14_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data15_OffsetPadding[15];
			uint8_t ___data15;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data15_OffsetPadding_forAlignmentOnly[15];
			uint8_t ___data15_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___ipv6_0;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___ipv6_0_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_1_OffsetPadding[1];
			uint8_t ___ipv6_1;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_1_OffsetPadding_forAlignmentOnly[1];
			uint8_t ___ipv6_1_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_2_OffsetPadding[2];
			uint8_t ___ipv6_2;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_2_OffsetPadding_forAlignmentOnly[2];
			uint8_t ___ipv6_2_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_3_OffsetPadding[3];
			uint8_t ___ipv6_3;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_3_OffsetPadding_forAlignmentOnly[3];
			uint8_t ___ipv6_3_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_4_OffsetPadding[4];
			uint8_t ___ipv6_4;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_4_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___ipv6_4_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_5_OffsetPadding[5];
			uint8_t ___ipv6_5;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_5_OffsetPadding_forAlignmentOnly[5];
			uint8_t ___ipv6_5_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_6_OffsetPadding[6];
			uint8_t ___ipv6_6;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_6_OffsetPadding_forAlignmentOnly[6];
			uint8_t ___ipv6_6_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_7_OffsetPadding[7];
			uint8_t ___ipv6_7;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_7_OffsetPadding_forAlignmentOnly[7];
			uint8_t ___ipv6_7_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_8_OffsetPadding[8];
			uint8_t ___ipv6_8;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_8_OffsetPadding_forAlignmentOnly[8];
			uint8_t ___ipv6_8_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_9_OffsetPadding[9];
			uint8_t ___ipv6_9;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_9_OffsetPadding_forAlignmentOnly[9];
			uint8_t ___ipv6_9_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_10_OffsetPadding[10];
			uint8_t ___ipv6_10;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_10_OffsetPadding_forAlignmentOnly[10];
			uint8_t ___ipv6_10_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_11_OffsetPadding[11];
			uint8_t ___ipv6_11;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_11_OffsetPadding_forAlignmentOnly[11];
			uint8_t ___ipv6_11_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_12_OffsetPadding[12];
			uint8_t ___ipv6_12;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_12_OffsetPadding_forAlignmentOnly[12];
			uint8_t ___ipv6_12_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_13_OffsetPadding[13];
			uint8_t ___ipv6_13;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_13_OffsetPadding_forAlignmentOnly[13];
			uint8_t ___ipv6_13_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_14_OffsetPadding[14];
			uint8_t ___ipv6_14;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_14_OffsetPadding_forAlignmentOnly[14];
			uint8_t ___ipv6_14_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_15_OffsetPadding[15];
			uint8_t ___ipv6_15;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_15_OffsetPadding_forAlignmentOnly[15];
			uint8_t ___ipv6_15_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___ipv4_0;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___ipv4_0_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv4_1_OffsetPadding[1];
			uint8_t ___ipv4_1;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv4_1_OffsetPadding_forAlignmentOnly[1];
			uint8_t ___ipv4_1_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv4_2_OffsetPadding[2];
			uint8_t ___ipv4_2;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv4_2_OffsetPadding_forAlignmentOnly[2];
			uint8_t ___ipv4_2_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv4_3_OffsetPadding[3];
			uint8_t ___ipv4_3;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv4_3_OffsetPadding_forAlignmentOnly[3];
			uint8_t ___ipv4_3_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___port0_OffsetPadding[16];
			uint8_t ___port0;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___port0_OffsetPadding_forAlignmentOnly[16];
			uint8_t ___port0_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___port1_OffsetPadding[17];
			uint8_t ___port1;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___port1_OffsetPadding_forAlignmentOnly[17];
			uint8_t ___port1_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___family_OffsetPadding[18];
			uint8_t ___family;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___family_OffsetPadding_forAlignmentOnly[18];
			uint8_t ___family_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ____padding_OffsetPadding[19];
			uint8_t ____padding;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ____padding_OffsetPadding_forAlignmentOnly[19];
			uint8_t ____padding_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___ipv6_scope_id_OffsetPadding[20];
			uint32_t ___ipv6_scope_id;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___ipv6_scope_id_OffsetPadding_forAlignmentOnly[20];
			uint32_t ___ipv6_scope_id_forAlignmentOnly;
		};
	};
};
struct Block_tBD2149E220F819A66A4A0A18F237932ACD9011DE 
{
	int32_t ___offset;
	int32_t ___count;
};
struct U3CshaderPassNamesU3Ee__FixedBuffer_t5EDC823777BDDC9D50E55FF3779FBC1B3820126D 
{
	union
	{
		struct
		{
			int32_t ___FixedElementField;
		};
		uint8_t U3CshaderPassNamesU3Ee__FixedBuffer_t5EDC823777BDDC9D50E55FF3779FBC1B3820126D__padding[64];
	};
};
struct SplitInfo_t708E0734C9BC407BA5882105A9721756605C913A 
{
	int32_t ___packetCount;
};
struct GeoPoolEntrySlot_tE1B7B103A90E4C3379CED82C8D6DA55189422977 
{
	uint32_t ___refCount;
	uint32_t ___hash;
	int32_t ___geoSlotHandle;
};
struct SubQueueItem_tAE6DA2A0B68107490190DB4680442767B9D420B7 
{
	int32_t ___connection;
	int32_t ___idx;
};
struct PipelineImpl_t422927EB35F759F18ECBA1C7B075AAC92918017E 
{
	int32_t ___FirstStageIndex;
	int32_t ___NumStages;
	int32_t ___receiveBufferOffset;
	int32_t ___sendBufferOffset;
	int32_t ___sharedBufferOffset;
	int32_t ___headerCapacity;
	int32_t ___payloadCapacity;
};
struct ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 
{
	int32_t ___Offset;
	int32_t ___Size;
};
struct ContactId_t3A8A09CF8F109FB8BFA43C877969F29EDEEF467B 
{
	int32_t ___m_IndexId;
	uint16_t ___m_WorldId;
	uint16_t ___m_Padding;
	int32_t ___m_GenerationId;
};
struct CompiledResourceInfo_t22204344249241C372CFC608709931F7EEEF733C 
{
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73* ___producers;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73* ___consumers;
	int32_t ___refCount;
	bool ___imported;
};
struct CompiledResourceInfo_t22204344249241C372CFC608709931F7EEEF733C_marshaled_pinvoke
{
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73* ___producers;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73* ___consumers;
	int32_t ___refCount;
	int32_t ___imported;
};
struct CompiledResourceInfo_t22204344249241C372CFC608709931F7EEEF733C_marshaled_com
{
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73* ___producers;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73* ___consumers;
	int32_t ___refCount;
	int32_t ___imported;
};
struct U3Cm_CullingPlanesU3Ee__FixedBuffer_tC12F25D54F08F8DA4BD1129A6E4B09385A888B62 
{
	union
	{
		struct
		{
			uint8_t ___FixedElementField;
		};
		uint8_t U3Cm_CullingPlanesU3Ee__FixedBuffer_tC12F25D54F08F8DA4BD1129A6E4B09385A888B62__padding[160];
	};
};
struct U3CbucketOffsetsU3Ee__FixedBuffer_t02430A98A7BDAED51F45AADE83044D404F68DB64 
{
	union
	{
		struct
		{
			uint32_t ___FixedElementField;
		};
		uint8_t U3CbucketOffsetsU3Ee__FixedBuffer_t02430A98A7BDAED51F45AADE83044D404F68DB64__padding[64];
	};
};
struct U3CbucketSizesU3Ee__FixedBuffer_t6E55B56B1212BEF70964F4027D41AAE340D5F91B 
{
	union
	{
		struct
		{
			uint8_t ___FixedElementField;
		};
		uint8_t U3CbucketSizesU3Ee__FixedBuffer_t6E55B56B1212BEF70964F4027D41AAE340D5F91B__padding[16];
	};
};
struct U3CdecodeTableU3Ee__FixedBuffer_t258FFC7B1E65D42A1149FE37642F704C0EC14C10 
{
	union
	{
		struct
		{
			uint16_t ___FixedElementField;
		};
		uint8_t U3CdecodeTableU3Ee__FixedBuffer_t258FFC7B1E65D42A1149FE37642F704C0EC14C10__padding[128];
	};
};
struct U3CencodeTableU3Ee__FixedBuffer_tE77A05CDD75344674029C3FA7C3BEF244EE1A840 
{
	union
	{
		struct
		{
			uint16_t ___FixedElementField;
		};
		uint8_t U3CencodeTableU3Ee__FixedBuffer_tE77A05CDD75344674029C3FA7C3BEF244EE1A840__padding[32];
	};
};
struct Rune_tE8DB196113D1DBF48E710C120CC50D2AD7D5915E 
{
	int32_t ___value;
};
struct U3CdataU3Ee__FixedBuffer_t35C35C5485EB6B7C62FF85A3530B348BB20B550F 
{
	union
	{
		struct
		{
			uint8_t ___FixedElementField;
		};
		uint8_t U3CdataU3Ee__FixedBuffer_t35C35C5485EB6B7C62FF85A3530B348BB20B550F__padding[1472];
	};
};
struct U3CPacketU3Ee__FixedBuffer_t55E02F8104D901D9FE76233288015124810472C7 
{
	union
	{
		struct
		{
			uint8_t ___FixedElementField;
		};
		uint8_t U3CPacketU3Ee__FixedBuffer_t55E02F8104D901D9FE76233288015124810472C7__padding[1472];
	};
};
struct UnsafeList_1_t5C65DCA6782B7C9860C859C2F0C07A2C497E822D 
{
	uint8_t* ___Ptr;
	int32_t ___m_length;
	int32_t ___m_capacity;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Allocator;
	int32_t ___padding;
};
struct UnsafeList_1_t239C74D869DA59B0CA5E63716E71208DEC68A70B 
{
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8* ___Ptr;
	int32_t ___m_length;
	int32_t ___m_capacity;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Allocator;
	int32_t ___padding;
};
struct UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 
{
	Il2CppFullySharedGenericStruct* ___Ptr;
	int32_t ___m_length;
	int32_t ___m_capacity;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Allocator;
	int32_t ___padding;
};
struct UnsafeList_1_tBB6961066733E78B8C22E5C4D4D5FF9A581981D1 
{
	Info_tDBEB127ABB26184014A541C0CAD1FC8D1B95DE84* ___Ptr;
	int32_t ___m_length;
	int32_t ___m_capacity;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Allocator;
	int32_t ___padding;
};
struct Allocator_t996642592271AAD9EE688F142741D512C07B5824 
{
	int32_t ___value__;
};
struct AsyncGPUReadbackRequest_t6A735D3E0F1DEF8F43EBD0E6FE550FAE564519C7 
{
	intptr_t ___m_Ptr;
	int32_t ___m_Version;
};
struct BatchCullingViewType_tAC2682BF9A489DF44A8960693303B47248C252CF 
{
	int32_t ___value__;
};
struct BatchDrawCommandFlags_tC502FA322382A3181F0800B1EA5A5654027FE034 
{
	int32_t ___value__;
};
struct BlendState_tC9B817349E49EF26CBCDC8FCE02789A661DC2630 
{
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState0;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState1;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState2;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState3;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState4;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState5;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState6;
	RenderTargetBlendState_t51229A3F09EE698F8E49731F1BC5BB77DBEDC4C7 ___m_BlendState7;
	uint8_t ___m_SeparateMRTBlendStates;
	uint8_t ___m_AlphaToMask;
	int16_t ___m_Padding;
};
struct Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Center;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Extents;
};
struct BuiltinRenderTextureType_t3D56813CAC7C6E4AC3B438039BD1CE7E62FE7C4E 
{
	int32_t ___value__;
};
struct ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0 
{
	bool ___isValid;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___pageAndID;
};
struct ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_pinvoke
{
	int32_t ___isValid;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___pageAndID;
};
struct ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_com
{
	int32_t ___isValid;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___pageAndID;
};
struct CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7  : public RuntimeObject
{
	intptr_t ___m_Ptr;
};
struct ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233  : public RuntimeObject
{
	intptr_t ___m_Ptr;
};
struct CubemapFace_t300D6E2CD7DF60D44AA28338748B607677ED1D1B 
{
	int32_t ___value__;
};
struct CullMode_t049B71889E4E981866E205A3F71DC8B856306D50 
{
	int32_t ___value__;
};
struct CullingResults_tD6B7EF20B68D47DFF3A99EB2EA73F47F1D460267 
{
	intptr_t ___ptr;
	CullingAllocationInfo_tB260F5CD0B290F74E145EB16E54B901CC68D9D5A* ___m_AllocationInfo;
};
struct Delegate_t  : public RuntimeObject
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	RuntimeObject* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	bool ___method_is_virtual;
};
struct Delegate_t_marshaled_pinvoke
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct Delegate_t_marshaled_com
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct DisconnectReason_tFE5B10DED724BFDB7E71A454B809E0CC73B6C36C 
{
	uint8_t ___value__;
};
struct DistanceMetric_t071B9815BB961E33F7CA2C553CA725F61AE09EDE 
{
	int32_t ___value__;
};
struct DrawRendererFlags_t3AD0574208BFF93F323D5E1E92012F19EAE972CD 
{
	int32_t ___value__;
};
struct ExtendedFeatureFlags_t736CECC24E1DDF78CDF8A2B134DFF9B1CA2BE01F 
{
	int32_t ___value__;
};
struct FalloffType_tE9BECCB411DA63109760103AF7476F422A01376D 
{
	uint8_t ___value__;
};
struct FilteringSettings_t75860B12A7BCF9A0E2F13CB2C2E5DCD9E1EEAD9F 
{
	RenderQueueRange_t7518252AA6426B1EA45D3D9B394F304EEF784D71 ___m_RenderQueueRange;
	int32_t ___m_LayerMask;
	uint32_t ___m_RenderingLayerMask;
	uint32_t ___m_BatchLayerMask;
	int32_t ___m_ExcludeMotionVectorObjects;
	int32_t ___m_ForceAllMotionVectorObjects;
	SortingLayerRange_t96D04CFB4E8824978FEB2CFFFCFEAC37E56D52C9 ___m_SortingLayerRange;
};
#pragma pack(push, tp, 1)
struct FixedBytes126_tC223222E11A3E93A15FE1C62C3429FC169DBC989 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0000;
				};
				#pragma pack(pop, tp)
				struct
				{
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0000_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0016_OffsetPadding[16];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0016;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0016_OffsetPadding_forAlignmentOnly[16];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0016_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0032_OffsetPadding[32];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0032;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0032_OffsetPadding_forAlignmentOnly[32];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0032_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0048_OffsetPadding[48];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0048;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0048_OffsetPadding_forAlignmentOnly[48];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0048_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0064_OffsetPadding[64];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0064;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0064_OffsetPadding_forAlignmentOnly[64];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0064_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0080_OffsetPadding[80];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0080;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0080_OffsetPadding_forAlignmentOnly[80];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0080_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0096_OffsetPadding[96];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0096;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0096_OffsetPadding_forAlignmentOnly[96];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0096_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0112_OffsetPadding[112];
					uint8_t ___byte0112;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0112_OffsetPadding_forAlignmentOnly[112];
					uint8_t ___byte0112_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0113_OffsetPadding[113];
					uint8_t ___byte0113;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0113_OffsetPadding_forAlignmentOnly[113];
					uint8_t ___byte0113_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0114_OffsetPadding[114];
					uint8_t ___byte0114;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0114_OffsetPadding_forAlignmentOnly[114];
					uint8_t ___byte0114_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0115_OffsetPadding[115];
					uint8_t ___byte0115;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0115_OffsetPadding_forAlignmentOnly[115];
					uint8_t ___byte0115_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0116_OffsetPadding[116];
					uint8_t ___byte0116;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0116_OffsetPadding_forAlignmentOnly[116];
					uint8_t ___byte0116_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0117_OffsetPadding[117];
					uint8_t ___byte0117;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0117_OffsetPadding_forAlignmentOnly[117];
					uint8_t ___byte0117_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0118_OffsetPadding[118];
					uint8_t ___byte0118;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0118_OffsetPadding_forAlignmentOnly[118];
					uint8_t ___byte0118_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0119_OffsetPadding[119];
					uint8_t ___byte0119;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0119_OffsetPadding_forAlignmentOnly[119];
					uint8_t ___byte0119_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0120_OffsetPadding[120];
					uint8_t ___byte0120;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0120_OffsetPadding_forAlignmentOnly[120];
					uint8_t ___byte0120_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0121_OffsetPadding[121];
					uint8_t ___byte0121;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0121_OffsetPadding_forAlignmentOnly[121];
					uint8_t ___byte0121_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0122_OffsetPadding[122];
					uint8_t ___byte0122;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0122_OffsetPadding_forAlignmentOnly[122];
					uint8_t ___byte0122_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0123_OffsetPadding[123];
					uint8_t ___byte0123;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0123_OffsetPadding_forAlignmentOnly[123];
					uint8_t ___byte0123_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0124_OffsetPadding[124];
					uint8_t ___byte0124;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0124_OffsetPadding_forAlignmentOnly[124];
					uint8_t ___byte0124_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0125_OffsetPadding[125];
					uint8_t ___byte0125;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0125_OffsetPadding_forAlignmentOnly[125];
					uint8_t ___byte0125_forAlignmentOnly;
				};
			};
		};
		uint8_t FixedBytes126_tC223222E11A3E93A15FE1C62C3429FC169DBC989__padding[126];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct FixedBytes32Align8_t07C7D543B487721FF9B88AD85209956AE423A779 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0000;
				};
				#pragma pack(pop, tp)
				struct
				{
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0000_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0016_OffsetPadding[16];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0016;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0016_OffsetPadding_forAlignmentOnly[16];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0016_forAlignmentOnly;
				};
			};
		};
		uint8_t FixedBytes32Align8_t07C7D543B487721FF9B88AD85209956AE423A779__padding[32];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct FixedBytes510_t95B284C3FF966246998B23701C3F0F55C6BD7973 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0000;
				};
				#pragma pack(pop, tp)
				struct
				{
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0000_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0016_OffsetPadding[16];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0016;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0016_OffsetPadding_forAlignmentOnly[16];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0016_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0032_OffsetPadding[32];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0032;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0032_OffsetPadding_forAlignmentOnly[32];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0032_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0048_OffsetPadding[48];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0048;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0048_OffsetPadding_forAlignmentOnly[48];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0048_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0064_OffsetPadding[64];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0064;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0064_OffsetPadding_forAlignmentOnly[64];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0064_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0080_OffsetPadding[80];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0080;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0080_OffsetPadding_forAlignmentOnly[80];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0080_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0096_OffsetPadding[96];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0096;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0096_OffsetPadding_forAlignmentOnly[96];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0096_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0112_OffsetPadding[112];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0112;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0112_OffsetPadding_forAlignmentOnly[112];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0112_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0128_OffsetPadding[128];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0128;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0128_OffsetPadding_forAlignmentOnly[128];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0128_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0144_OffsetPadding[144];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0144;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0144_OffsetPadding_forAlignmentOnly[144];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0144_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0160_OffsetPadding[160];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0160;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0160_OffsetPadding_forAlignmentOnly[160];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0160_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0176_OffsetPadding[176];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0176;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0176_OffsetPadding_forAlignmentOnly[176];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0176_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0192_OffsetPadding[192];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0192;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0192_OffsetPadding_forAlignmentOnly[192];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0192_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0208_OffsetPadding[208];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0208;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0208_OffsetPadding_forAlignmentOnly[208];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0208_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0224_OffsetPadding[224];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0224;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0224_OffsetPadding_forAlignmentOnly[224];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0224_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0240_OffsetPadding[240];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0240;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0240_OffsetPadding_forAlignmentOnly[240];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0240_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0256_OffsetPadding[256];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0256;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0256_OffsetPadding_forAlignmentOnly[256];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0256_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0272_OffsetPadding[272];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0272;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0272_OffsetPadding_forAlignmentOnly[272];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0272_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0288_OffsetPadding[288];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0288;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0288_OffsetPadding_forAlignmentOnly[288];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0288_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0304_OffsetPadding[304];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0304;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0304_OffsetPadding_forAlignmentOnly[304];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0304_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0320_OffsetPadding[320];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0320;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0320_OffsetPadding_forAlignmentOnly[320];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0320_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0336_OffsetPadding[336];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0336;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0336_OffsetPadding_forAlignmentOnly[336];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0336_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0352_OffsetPadding[352];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0352;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0352_OffsetPadding_forAlignmentOnly[352];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0352_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0368_OffsetPadding[368];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0368;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0368_OffsetPadding_forAlignmentOnly[368];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0368_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0384_OffsetPadding[384];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0384;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0384_OffsetPadding_forAlignmentOnly[384];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0384_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0400_OffsetPadding[400];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0400;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0400_OffsetPadding_forAlignmentOnly[400];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0400_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0416_OffsetPadding[416];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0416;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0416_OffsetPadding_forAlignmentOnly[416];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0416_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0432_OffsetPadding[432];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0432;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0432_OffsetPadding_forAlignmentOnly[432];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0432_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0448_OffsetPadding[448];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0448;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0448_OffsetPadding_forAlignmentOnly[448];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0448_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0464_OffsetPadding[464];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0464;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0464_OffsetPadding_forAlignmentOnly[464];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0464_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0480_OffsetPadding[480];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0480;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0480_OffsetPadding_forAlignmentOnly[480];
					FixedBytes16_tBBD888116CBD6329886E0FE97A82EEB4B7CB3FA0 ___offset0480_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0496_OffsetPadding[496];
					uint8_t ___byte0496;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0496_OffsetPadding_forAlignmentOnly[496];
					uint8_t ___byte0496_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0497_OffsetPadding[497];
					uint8_t ___byte0497;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0497_OffsetPadding_forAlignmentOnly[497];
					uint8_t ___byte0497_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0498_OffsetPadding[498];
					uint8_t ___byte0498;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0498_OffsetPadding_forAlignmentOnly[498];
					uint8_t ___byte0498_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0499_OffsetPadding[499];
					uint8_t ___byte0499;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0499_OffsetPadding_forAlignmentOnly[499];
					uint8_t ___byte0499_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0500_OffsetPadding[500];
					uint8_t ___byte0500;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0500_OffsetPadding_forAlignmentOnly[500];
					uint8_t ___byte0500_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0501_OffsetPadding[501];
					uint8_t ___byte0501;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0501_OffsetPadding_forAlignmentOnly[501];
					uint8_t ___byte0501_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0502_OffsetPadding[502];
					uint8_t ___byte0502;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0502_OffsetPadding_forAlignmentOnly[502];
					uint8_t ___byte0502_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0503_OffsetPadding[503];
					uint8_t ___byte0503;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0503_OffsetPadding_forAlignmentOnly[503];
					uint8_t ___byte0503_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0504_OffsetPadding[504];
					uint8_t ___byte0504;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0504_OffsetPadding_forAlignmentOnly[504];
					uint8_t ___byte0504_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0505_OffsetPadding[505];
					uint8_t ___byte0505;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0505_OffsetPadding_forAlignmentOnly[505];
					uint8_t ___byte0505_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0506_OffsetPadding[506];
					uint8_t ___byte0506;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0506_OffsetPadding_forAlignmentOnly[506];
					uint8_t ___byte0506_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0507_OffsetPadding[507];
					uint8_t ___byte0507;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0507_OffsetPadding_forAlignmentOnly[507];
					uint8_t ___byte0507_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0508_OffsetPadding[508];
					uint8_t ___byte0508;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0508_OffsetPadding_forAlignmentOnly[508];
					uint8_t ___byte0508_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___byte0509_OffsetPadding[509];
					uint8_t ___byte0509;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___byte0509_OffsetPadding_forAlignmentOnly[509];
					uint8_t ___byte0509_forAlignmentOnly;
				};
			};
		};
		uint8_t FixedBytes510_t95B284C3FF966246998B23701C3F0F55C6BD7973__padding[510];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct FixedBytes64Align8_t84631A2A3E4A6CEF77C84D9B630BDF9720B945E1 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0000;
				};
				#pragma pack(pop, tp)
				struct
				{
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0000_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0016_OffsetPadding[16];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0016;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0016_OffsetPadding_forAlignmentOnly[16];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0016_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0032_OffsetPadding[32];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0032;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0032_OffsetPadding_forAlignmentOnly[32];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0032_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___offset0048_OffsetPadding[48];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0048;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___offset0048_OffsetPadding_forAlignmentOnly[48];
					FixedBytes16Align8_t94D49B0852778B92D3912ABC4979B11ADF6ECEE4 ___offset0048_forAlignmentOnly;
				};
			};
		};
		uint8_t FixedBytes64Align8_t84631A2A3E4A6CEF77C84D9B630BDF9720B945E1__padding[64];
	};
};
#pragma pack(pop, tp)
struct GCHandle_tC44F6F72EE68BD4CFABA24309DA7A179D41127DC 
{
	intptr_t ___handle;
};
struct GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1  : public RuntimeObject
{
	intptr_t ___m_Ptr;
};
struct GraphicsFenceType_t8C3F229DC2BE62FA78724BA3D35FFCB3A64F7E0A 
{
	int32_t ___value__;
};
struct GraphicsFormat_tC3D1898F3F3F1F57256C7F3FFD6BA9A37AE7E713 
{
	int32_t ___value__;
};
struct InstanceOcclusionEventType_tF66ED25A6A1D3943D326795EA91E803D666ECF79 
{
	int32_t ___value__;
};
struct LightShadows_t5A3719FE33F8D536E5785AC42B4DF6E6F19666EA 
{
	int32_t ___value__;
};
struct LightType_t2D4D43054E7473EECEB54493C0055AE074780234 
{
	int32_t ___value__;
};
struct LightmapBakeType_tD6FF28E59BAAD80648796C2835AB8DC0B0F8B232 
{
	int32_t ___value__;
};
struct Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D 
{
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f0;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f1;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f2;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f3;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f4;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f5;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f6;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f7;
};
struct MeshTopology_t815FF5CF04D62195A23E2DF8A5C0A071F11FBCBF 
{
	int32_t ___value__;
};
struct MotionVectorGenerationMode_tE87C61556749260EF5429A0F8FE55DAD30EEAFCB 
{
	int32_t ___value__;
};
struct NetworkConnection_t0A1170D9665C62249582E5DAABC2EAF2D01DEDF4 
{
	ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB ___m_ConnectionId;
};
struct NetworkEndPoint_tD248F1CF54F01EF67B92B12963FC7060054C4CBC 
{
	Baselib_NetworkAddress_t2F4AF92B4EEFE31182BADF512CA004AFF48128E0 ___rawNetworkAddress;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C  : public RuntimeObject
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
	intptr_t ___m_CachedPtr;
};
struct OcclusionTest_tFF967683F812A2788B5C8A9A220B83B43060435E 
{
	int32_t ___value__;
};
struct PerObjectData_t04DDCBE9ABF1113E8F9BAFCF4A7F94DD841B9CC9 
{
	int32_t ___value__;
};
struct PhysicsRotate_tC2E9C9A6A76056D2DA0765D0FF6D3A4AA3EE0A0F 
{
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___direction;
};
struct Plane_tB7D8CC6F7AACF5F3AA483AF005C1102A8577BC0C 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Normal;
	float ___m_Distance;
};
struct RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Point;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Normal;
	uint32_t ___m_FaceID;
	float ___m_Distance;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_UV;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_Collider;
};
struct RaycastHit2D_t3EAAA06E6603C6BC61AC1291DD881C5C1E23BDFA 
{
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_Centroid;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_Point;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_Normal;
	float ___m_Distance;
	float ___m_Fraction;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_Collider;
};
struct RenderGraphResourceType_t5F552AF06E38DEC5775B77F13C8783A895FCD086 
{
	int32_t ___value__;
};
struct RenderStateMask_tC9C95BF62EADEE4D622D4E16CDE1DF94E2A9EF57 
{
	int32_t ___value__;
};
struct RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 
{
	uintptr_t ___context;
	uint32_t ___index;
	uint32_t ___frame;
	uint32_t ___type;
	uint32_t ___contextID;
};
struct ScaleMode_t16AD656758EE54C56B3DA34FE4F2033C9C2EE13D 
{
	int32_t ___value__;
};
struct ShadowCastingMode_tF30806698B37CF120A1A506BD7549EAF308E7C6D 
{
	int32_t ___value__;
};
struct ShadowSplitData_tC276A96F461DD73CFF6D94DB557D42A1643640DF 
{
	int32_t ___m_CullingPlaneCount;
	U3Cm_CullingPlanesU3Ee__FixedBuffer_tC12F25D54F08F8DA4BD1129A6E4B09385A888B62 ___m_CullingPlanes;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___m_CullingSphere;
	float ___m_ShadowCascadeBlendCullingFactor;
	float ___m_CullingNearPlane;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___m_CullingMatrix;
};
struct SortingCriteria_t4907D221CB6E6AA4A32C1ED7B5D17103FD3E7C39 
{
	int32_t ___value__;
};
struct StoreReason_tB71F110DFEED47ED8929B7874BE46BD4AFA9D5AC 
{
	int32_t ___value__;
};
struct StreamCompressionModel_t7F9FB7DB2D88DA832A4DD61C1E6587A4C530697D 
{
	uint8_t ___m_Initialized;
	U3CencodeTableU3Ee__FixedBuffer_tE77A05CDD75344674029C3FA7C3BEF244EE1A840 ___encodeTable;
	U3CdecodeTableU3Ee__FixedBuffer_t258FFC7B1E65D42A1149FE37642F704C0EC14C10 ___decodeTable;
	U3CbucketSizesU3Ee__FixedBuffer_t6E55B56B1212BEF70964F4027D41AAE340D5F91B ___bucketSizes;
	U3CbucketOffsetsU3Ee__FixedBuffer_t02430A98A7BDAED51F45AADE83044D404F68DB64 ___bucketOffsets;
};
struct SubPassFlags_tB4066DF82B36110B6163EB5C3A48F49FD4DD3AE5 
{
	int32_t ___value__;
};
struct TextureFormat_t87A73E4A3850D3410DC211676FC14B94226C1C1D 
{
	int32_t ___value__;
};
struct TextureUVOriginSelection_t885C52C74C02CA1A0D3D9FDB10D05A1BFBCD7F81 
{
	int32_t ___value__;
};
struct TileAnimationFlags_tA70AD7E12D667AA759A54CBC19A42FCAF5BB2B79 
{
	int32_t ___value__;
};
struct TileFlags_tDCEE980FCB6A2159202B4C8096C11452E318D2A9 
{
	int32_t ___value__;
};
struct TransformUpdatePacket_t056014168D7AE17359B1BD85E70A6E1B43C3AB18 
{
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___localToWorld0;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___localToWorld1;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___localToWorld2;
};
struct TreeInstance_t382B018173ED020660D262061EA9424682614F50 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___position;
	float ___widthScale;
	float ___heightScale;
	float ___rotation;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___color;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___lightmapColor;
	int32_t ___prototypeIndex;
	float ___temporaryDistance;
};
struct UVChannel_t5E8646A77DA4545F4392E73F3FE85A89C852A35A 
{
	uint32_t ___value__;
};
struct UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500 
{
	uint8_t* ___Ptr;
	int32_t ___Length;
	int32_t ___Capacity;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Allocator;
	int32_t ___Alignment;
};
struct Vertex_t016AC68A2E6C62576E65412BEC71544AFC01AFC7 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___position;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___tint;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___uv;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___layoutUV;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___xformClipPages;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___ids;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___flags;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___opacityColorPages;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___settingIndex;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___circle;
	float ___textureId;
};
struct VertexAttribute_tF34C1B76F20CA4AEC9D606BCD37A8A0C4A24C9A6 
{
	int32_t ___value__;
};
struct VertexAttributeFormat_tD714C51E671502B116ACE5E23F042BA80649D32F 
{
	int32_t ___value__;
};
struct VisibleLightFlags_t337DB92EFB0014AD6A250E1E45338B1194657CD8 
{
	int32_t ___value__;
};
struct float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2 
{
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___c0;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___c1;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___c2;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___c3;
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			bool ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			bool ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			Il2CppChar ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			Il2CppChar ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_marshaled_pinvoke
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_marshaled_com
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct quaternion_tD6BCBECAF088B9EBAE2345EC8534C7A1A4C910D4 
{
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___value;
};
struct TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE 
{
	intptr_t ___function;
	intptr_t ___state;
};
struct Baselib_RegisteredNetwork_BufferSlice_tBEE9A8BFFB4625CEE8B4AD97E30DFE397D34F273 
{
	intptr_t ___id;
	intptr_t ___data;
	uint32_t ___size;
	uint32_t ___offset;
};
struct Baselib_RegisteredNetwork_CompletionStatus_t641158FAD66BA4045ABBCFEB080AC2397346D6C7 
{
	int32_t ___value__;
};
struct Allocation_tB22CAD44C5AF0C13B3D441F8B419C9BA1D19F70C 
{
	int32_t ___handle;
	Block_tBD2149E220F819A66A4A0A18F237932ACD9011DE ___block;
};
struct PlanePacket4_t2954005DBF78AC180CF45B652536CC2F5158D54B 
{
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___nx;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___ny;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___nz;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___d;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___nxAbs;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___nyAbs;
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___nzAbs;
};
struct IPCData_t46E9604572E2A38CBBA8A418C6E25E6BAE2757CC 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			uint16_t ___fromPort;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint16_t ___fromPort_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___length_OffsetPadding[2];
			int32_t ___length;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___length_OffsetPadding_forAlignmentOnly[2];
			int32_t ___length_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___data_OffsetPadding[6];
			U3CdataU3Ee__FixedBuffer_t35C35C5485EB6B7C62FF85A3530B348BB20B550F ___data;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___data_OffsetPadding_forAlignmentOnly[6];
			U3CdataU3Ee__FixedBuffer_t35C35C5485EB6B7C62FF85A3530B348BB20B550F ___data_forAlignmentOnly;
		};
	};
};
struct AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 
{
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___cameraID;
	JobHandle_t5DF5F99902FED3C801A81C05205CEA6CE039EF08 ___jobHandle;
};
struct HitEntry_tB5C47A91F398525506E16A47748007BC0EF8FD4C 
{
	uint32_t ___instanceID;
	uint32_t ___primitiveIndex;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___barycentrics;
};
struct MaterialFlags_t9F64C8E8BB3179F4636BA8E5E8C473D9623AB173 
{
	uint32_t ___value__;
};
struct NativeColorPage_tD578D4E96ABBDBB0E6A608F6CE97A0362ABD7D4B 
{
	int32_t ___isValid;
	Color32_t73C5004937BF5BB8AD55323D51AAA40A898EF48B ___pageAndID;
};
struct BackgroundRepeatInstance_t2D7A8E1F6278188BE2026DF769C49A975D38B12D 
{
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___rect;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___backgroundRepeatRect;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___uv;
};
struct OccluderContextSlot_t963DBFFF1612E4108D0BEB42369F78758BE71D5D 
{
	bool ___valid;
	int32_t ___lastUsedFrameIndex;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
};
struct OccluderContextSlot_t963DBFFF1612E4108D0BEB42369F78758BE71D5D_marshaled_pinvoke
{
	int32_t ___valid;
	int32_t ___lastUsedFrameIndex;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
};
struct OccluderContextSlot_t963DBFFF1612E4108D0BEB42369F78758BE71D5D_marshaled_com
{
	int32_t ___valid;
	int32_t ___lastUsedFrameIndex;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
};
struct TransformWriteMode_t9006A15DA105F005CFBC6CC59DAD4540C67046B1 
{
	int32_t ___value__;
};
struct ContactBeginEvent_t4F38390723F5E35F57A4009B9CACEF4CD212AA90 
{
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_ShapeA;
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_ShapeB;
	ContactId_t3A8A09CF8F109FB8BFA43C877969F29EDEEF467B ___m_ContactId;
};
struct ContactEndEvent_tE811C99069F0369669904C1229780EDB98CCD3AE 
{
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_ShapeA;
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_ShapeB;
	ContactId_t3A8A09CF8F109FB8BFA43C877969F29EDEEF467B ___m_ContactId;
};
struct JointThresholdEvent_t4F68DE6F2342237EE679E77E31F30F355EB2D808 
{
	PhysicsJoint_tD7FB5BFA5B2CF4F6A267F59B0038EE32EA89CE9D ___m_Joint;
	intptr_t ___m_UserData;
};
struct TriggerBeginEvent_t377C38E390AB920034AF6E063A969306047BD6B8 
{
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_TriggerShape;
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_VisitorShape;
};
struct TriggerEndEvent_t7FEED3B9C48823CEE1DBB1B83E237B5D485F0FD5 
{
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_TriggerShape;
	PhysicsShape_tB637EBCF69E3A5493C8E6F41F1332D1CD996291B ___m_VisitorShape;
};
struct DrawFillOptions_tA768E6FD7A9C80D5E762C8DE13C9D48529D63130 
{
	int32_t ___value__;
};
struct Brick_tE6E9230DFDF650A631C116E79FB28F41618C3CE0 
{
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___position;
	int32_t ___subdivisionLevel;
};
struct SplitInfo_tBD1436BC99CBBC9658FA9219EB22657F757C4A1A 
{
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___receiverSphereLightSpace;
	float ___cascadeBlendCullingFactor;
};
struct LightData_tAC4023737E9903DE3F96B993AA323E062ABCB9ED 
{
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___position;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___color;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___attenuation;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___spotDirection;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___occlusionProbeChannels;
	uint32_t ___layerMask;
};
struct Slot_tA2F29CF08EAE46C3E2B6D96DCD7C96BF887A6127 
{
	bool ___isActive;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
	int32_t ___planeCount;
	int32_t ___lastUsedFrameIndex;
};
struct Slot_tA2F29CF08EAE46C3E2B6D96DCD7C96BF887A6127_marshaled_pinvoke
{
	int32_t ___isActive;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
	int32_t ___planeCount;
	int32_t ___lastUsedFrameIndex;
};
struct Slot_tA2F29CF08EAE46C3E2B6D96DCD7C96BF887A6127_marshaled_com
{
	int32_t ___isActive;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
	int32_t ___planeCount;
	int32_t ___lastUsedFrameIndex;
};
struct PendingSend_tC3CE84554C44A80712A3DA2E69BD887F0300440E 
{
	ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB ___Connection;
	int32_t ___BufferIndex;
};
struct ColliderType_t4C26FB284A330CAD69E47FFD734C9C778E077692 
{
	int32_t ___value__;
};
struct OutputVertex_tA4BCEA851534A5A22BE19AD8D0F20102B11F88C4 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___Position;
};
struct SettingsOptions_t935A71692F21EE9D959FCAC00574AA018F8BA2CB 
{
	uint16_t ___value__;
};
struct PointElement_tFE435D22E766645936C545EC265D1E41C047D9DE 
{
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___position;
	float ___radius;
	float ___elementDepth;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___color;
};
struct Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F 
{
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f0;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f1;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f2;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f3;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f4;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f5;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f6;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f7;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f8;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f9;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f10;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f11;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f12;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f13;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f14;
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___f15;
};
struct FixedList32Bytes_1_t02D797FE0EF11E83016BA95DD7B282490C8A472E 
{
	alignas(8) FixedBytes32Align8_t07C7D543B487721FF9B88AD85209956AE423A779 ___data;
};
struct FixedList64Bytes_1_t137BDA0D26652E438404CA31731069295DAC8E1C 
{
	alignas(8) FixedBytes64Align8_t84631A2A3E4A6CEF77C84D9B630BDF9720B945E1 ___data;
};
struct NativeArray_1_t60C2883F9133642CF381234B3DD254605DD4F0A5 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tB7D9C79EBF3AEA35F54C11110B41E68AE0A214F6 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t79AF901C035FC0E2DB95A6C857E2A6FE26138AF6 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t107C57D0357BCF9956A60495CD8FAADDF1D26AFB 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t2FF0F1BBD323229A2F9ABD52397EA6EF9B105E09 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t0215017C25A7FD4BE98E0A8000F004E7D119560D 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t1864D78BA2DD264CF10C4F534C4A53B8D45EDFC3 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t712BAFB3B7FD80607F953479B6E3EE2D54BD9446 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t727E0B11B40EA7D6477F67D79DA7B7F7C383735E 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t703E60DA65EBF076EB4916D3B9D9987A5A09B9CC 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t8E042B4249B3126F27EE49887D2507798DC25F2C 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t25F6CEC65DB0532CB91E2B2890FF6C2D52A210A3 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tD5B3D215346B1527FA237140626E9D632A0EE488 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t8E236FFFE7B598011354300BFCED0A15647BBC5D 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t0AB49EE6A37F6BC668C15EDFBE9BE92A22B2F0DB 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t5576C5C8F17BB3E1BA11BBA3EC50A55FC1246445 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t9B7A94FA050F43A3996B812B9164E7885F38ADC3 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tD50C1560C0B57B735333075DF206AB11B0E18565 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t275C00CC374DEA66C69B3BB3992116F315A8E934 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tFBC4A05617FFC862FFD574140F343DA82BF818B2 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_t497DD754C21E03BAC4C1F2BB7A3BB0025FF4EB88 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tF0A83538A02306EA7A8F22FEA945A6961365E8C8 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tD66836534F00AADA4D14B93A8662AF8DA2D65075 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 
{
	union
	{
		struct
		{
			uint16_t ___utf8LengthInBytes;
			alignas(1) FixedBytes126_tC223222E11A3E93A15FE1C62C3429FC169DBC989 ___bytes;
		};
		uint8_t FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952__padding[128];
	};
};
struct FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E 
{
	union
	{
		struct
		{
			uint16_t ___utf8LengthInBytes;
			alignas(1) FixedBytes510_t95B284C3FF966246998B23701C3F0F55C6BD7973 ___bytes;
		};
		uint8_t FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E__padding[512];
	};
};
struct GraphicsFence_t199180163AEDE0C1BE868F8E1314A47610B1FABB 
{
	intptr_t ___m_Ptr;
	int32_t ___m_Version;
	int32_t ___m_FenceType;
};
struct Long512_t2D339FF6672EB3709A6C638EABA7D13C7FEC1878 
{
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f0;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f1;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f2;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f3;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f4;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f5;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f6;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f7;
};
struct MulticastDelegate_t  : public Delegate_t
{
	DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771* ___delegates;
};
struct MulticastDelegate_t_marshaled_pinvoke : public Delegate_t_marshaled_pinvoke
{
	Delegate_t_marshaled_pinvoke** ___delegates;
};
struct MulticastDelegate_t_marshaled_com : public Delegate_t_marshaled_com
{
	Delegate_t_marshaled_com** ___delegates;
};
struct PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F 
{
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___position;
	PhysicsRotate_tC2E9C9A6A76056D2DA0765D0FF6D3A4AA3EE0A0F ___rotation;
};
struct RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C 
{
	uint8_t ___layer;
	uint32_t ___renderingLayerMask;
	int32_t ___motionMode;
	int32_t ___shadowCastingMode;
	bool ___staticShadowCaster;
	int32_t ___rendererPriority;
	bool ___supportsIndirect;
};
struct RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_marshaled_pinvoke
{
	uint8_t ___layer;
	uint32_t ___renderingLayerMask;
	int32_t ___motionMode;
	int32_t ___shadowCastingMode;
	int32_t ___staticShadowCaster;
	int32_t ___rendererPriority;
	int32_t ___supportsIndirect;
};
struct RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_marshaled_com
{
	uint8_t ___layer;
	uint32_t ___renderingLayerMask;
	int32_t ___motionMode;
	int32_t ___shadowCastingMode;
	int32_t ___staticShadowCaster;
	int32_t ___rendererPriority;
	int32_t ___supportsIndirect;
};
struct RasterState_tA30E8336EA5D1E2152A6C7252F15384985B98A26 
{
	int32_t ___m_CullingMode;
	int32_t ___m_OffsetUnits;
	float ___m_OffsetFactor;
	uint8_t ___m_DepthClip;
	uint8_t ___m_Conservative;
	uint8_t ___m_Padding1;
	uint8_t ___m_Padding2;
};
struct RenderTargetIdentifier_tA528663AC6EB3911D8E91AA40F7070FA5455442B 
{
	int32_t ___m_Type;
	int32_t ___m_NameID;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_EntityId;
	intptr_t ___m_BufferPointer;
	int32_t ___m_MipLevel;
	int32_t ___m_CubeFace;
	int32_t ___m_DepthSlice;
};
struct RendererListLegacyResource_tEF05A444F7845E04F5E6568549AF26D434AD1B68 
{
	RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 ___rendererList;
	bool ___isActive;
};
struct RendererListLegacyResource_tEF05A444F7845E04F5E6568549AF26D434AD1B68_marshaled_pinvoke
{
	RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 ___rendererList;
	int32_t ___isActive;
};
struct RendererListLegacyResource_tEF05A444F7845E04F5E6568549AF26D434AD1B68_marshaled_com
{
	RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 ___rendererList;
	int32_t ___isActive;
};
struct ResourceHandle_tD3B1FFBD59EB9C23F0A020351836F834C4BD276C 
{
	uint32_t ___m_Value;
	int32_t ___m_Version;
	int32_t ___m_Type;
};
struct ResourceUnversionedData_t3F4B539E7806E513C53A94EAABF5F969AAA384CC 
{
	bool ___isImported;
	bool ___isShared;
	int32_t ___tag;
	int32_t ___lastUsePassID;
	int32_t ___lastWritePassID;
	int32_t ___firstUsePassID;
	bool ___memoryLess;
	int32_t ___width;
	int32_t ___height;
	int32_t ___volumeDepth;
	int32_t ___msaaSamples;
	int32_t ___graphicsFormat;
	int32_t ___latestVersionNumber;
	bool ___clear;
	bool ___discard;
	bool ___bindMS;
	int32_t ___textureUVOrigin;
};
struct ResourceUnversionedData_t3F4B539E7806E513C53A94EAABF5F969AAA384CC_marshaled_pinvoke
{
	int32_t ___isImported;
	int32_t ___isShared;
	int32_t ___tag;
	int32_t ___lastUsePassID;
	int32_t ___lastWritePassID;
	int32_t ___firstUsePassID;
	int32_t ___memoryLess;
	int32_t ___width;
	int32_t ___height;
	int32_t ___volumeDepth;
	int32_t ___msaaSamples;
	int32_t ___graphicsFormat;
	int32_t ___latestVersionNumber;
	int32_t ___clear;
	int32_t ___discard;
	int32_t ___bindMS;
	int32_t ___textureUVOrigin;
};
struct ResourceUnversionedData_t3F4B539E7806E513C53A94EAABF5F969AAA384CC_marshaled_com
{
	int32_t ___isImported;
	int32_t ___isShared;
	int32_t ___tag;
	int32_t ___lastUsePassID;
	int32_t ___lastWritePassID;
	int32_t ___firstUsePassID;
	int32_t ___memoryLess;
	int32_t ___width;
	int32_t ___height;
	int32_t ___volumeDepth;
	int32_t ___msaaSamples;
	int32_t ___graphicsFormat;
	int32_t ___latestVersionNumber;
	int32_t ___clear;
	int32_t ___discard;
	int32_t ___bindMS;
	int32_t ___textureUVOrigin;
};
struct ShadowSliceData_t1BCFEDC63BECA994949FE1F4245CEE930EE69E20 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___viewMatrix;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___projectionMatrix;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___shadowTransform;
	int32_t ___offsetX;
	int32_t ___offsetY;
	int32_t ___resolution;
	ShadowSplitData_tC276A96F461DD73CFF6D94DB557D42A1643640DF ___splitData;
};
struct SortingSettings_t506C3B318FDFD3C2B1620E9B951829C631137E72 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___m_WorldToCameraMatrix;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_CameraPosition;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_CustomAxis;
	int32_t ___m_Criteria;
	int32_t ___m_DistanceMetric;
};
struct StoreAudit_t9E8FF144788FDFF9C68E912C5BB87C533F860292 
{
	int32_t ___reason;
	int32_t ___passId;
	int32_t ___msaaReason;
	int32_t ___msaaPassId;
};
struct SubMeshDescriptor_t699E32E3F27A97CF89B0030F74C82D5FB7DEF934 
{
	Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 ___U3CboundsU3Ek__BackingField;
	int32_t ___U3CtopologyU3Ek__BackingField;
	int32_t ___U3CindexStartU3Ek__BackingField;
	int32_t ___U3CindexCountU3Ek__BackingField;
	int32_t ___U3CbaseVertexU3Ek__BackingField;
	int32_t ___U3CfirstVertexU3Ek__BackingField;
	int32_t ___U3CvertexCountU3Ek__BackingField;
};
struct SubPassDescriptor_t912FE0FF4C99BF293A1E4442353C35B2BB8997A9 
{
	AttachmentIndexArray_tDC550BA2CD14AFB3B5545B02331C26903ADA90B5 ___inputs;
	AttachmentIndexArray_tDC550BA2CD14AFB3B5545B02331C26903ADA90B5 ___colorOutputs;
	int32_t ___flags;
};
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct TileAnimationEntityIdData_t167D5D62289EB4EDFE19B374BF7E079DFE94677D 
{
	intptr_t ___m_AnimatedSpritesEntityIdPtr;
	int32_t ___m_Count;
	float ___m_AnimationSpeed;
	float ___m_AnimationStartTime;
	int32_t ___m_Flags;
};
struct TileData_tFB814629D010ABD175127C0BE96FD96EA606E00F 
{
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_Sprite;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_Color;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___m_Transform;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_GameObject;
	int32_t ___m_Flags;
	int32_t ___m_ColliderType;
};
struct UnsafeMeshGenerationNode_t1FF7D38D9B39DC59A2F8E2DFC92BFAB2481E88A2 
{
	GCHandle_tC44F6F72EE68BD4CFABA24309DA7A179D41127DC ___m_Handle;
};
struct VertexAttributeDescriptor_tD4231FBF57335465D16308D2A18E8E83D36BFA76 
{
	int32_t ___U3CattributeU3Ek__BackingField;
	int32_t ___U3CformatU3Ek__BackingField;
	int32_t ___U3CdimensionU3Ek__BackingField;
	int32_t ___U3CstreamU3Ek__BackingField;
};
struct VisibleLight_t0A4DF5B22865A00F618A0352B805277FA0132805 
{
	int32_t ___m_LightType;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_FinalColor;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___m_ScreenRect;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___m_LocalToWorldMatrix;
	float ___m_Range;
	float ___m_SpotAngle;
	float ___m_InnerSpotAngle;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_AreaSize;
	float ___m_ShapeRadius;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_EntityId;
	int32_t ___m_Flags;
};
struct VisibleReflectionProbe_t8AF1FAD09A36E33F5101B683DB8E99582815EF0B 
{
	Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 ___m_Bounds;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___m_LocalToWorldMatrix;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___m_HdrData;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Center;
	float ___m_BlendDistance;
	int32_t ___m_Importance;
	int32_t ___m_BoxProjection;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_EntityId;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_TextureId;
};
struct RTInstance_t82A289FDC8E8112219057734D0AE172A4E822AED 
{
	float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2 ___localToWorld;
	float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2 ___previousLocalToWorld;
	float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2 ___localToWorldNormals;
	uint32_t ___renderingLayerMask;
	uint32_t ___instanceMask;
	uint32_t ___userMaterialID;
	uint32_t ___geometryIndex;
};
struct ShadowResolutionRequest_tC1B869ADCA139D8D7AD8A355373F84C7F5E0FCBE 
{
	uint16_t ___visibleLightIndex;
	uint16_t ___perLightShadowSliceIndex;
	uint16_t ___requestedResolution;
	uint16_t ___offsetX;
	uint16_t ___offsetY;
	uint16_t ___allocatedResolution;
	uint16_t ___m_ShadowProperties;
};
struct Baselib_RegisteredNetwork_CompletionResult_t4C07F67F2F155FE19DBE7A41393038A154301CC7 
{
	int32_t ___status;
	uint32_t ___bytesTransferred;
	intptr_t ___requestUserdata;
};
struct Baselib_RegisteredNetwork_Endpoint_t9A38585FFD3425455A7E0AFC10F16BCD13B1F32C 
{
	Baselib_RegisteredNetwork_BufferSlice_tBEE9A8BFFB4625CEE8B4AD97E30DFE397D34F273 ___slice;
};
struct PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E 
{
	UnsafeList_1_t5C65DCA6782B7C9860C859C2F0C07A2C497E822D ___meshLods;
	UnsafeList_1_t5C65DCA6782B7C9860C859C2F0C07A2C497E822D ___crossFades;
};
struct IncomingDisconnection_tD1F2DDEC6E7112BF5659DFF4FFD8DE263CF7F294 
{
	ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB ___Connection;
	uint8_t ___Reason;
};
struct MeshChunk_tEEA891FD6A9FBF660DB8ED98B23FCCB8A413C2BD 
{
	Allocation_tB22CAD44C5AF0C13B3D441F8B419C9BA1D19F70C ___vertexAlloc;
	Allocation_tB22CAD44C5AF0C13B3D441F8B419C9BA1D19F70C ___indexAlloc;
};
struct Info_tA3039772991DEEDBC29A00439A055C5166133A27 
{
	int32_t ___viewType;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
	int32_t ___splitIndex;
};
struct Info_tDBEB127ABB26184014A541C0CAD1FC8D1B95DE84 
{
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___viewInstanceID;
	int32_t ___eventType;
	int32_t ___occluderVersion;
	int32_t ___subviewMask;
	int32_t ___occlusionTest;
};
struct Request_tAA55F47806E39B0E19B53273DCBFB5CF457F9187 
{
	UnsafeList_1_tBB6961066733E78B8C22E5C4D4D5FF9A581981D1 ___info;
	AsyncGPUReadbackRequest_t6A735D3E0F1DEF8F43EBD0E6FE550FAE564519C7 ___readback;
};
struct GpuMaterialEntry_t4A4728FB9206E4E82C457E3339E0E00FD904518F 
{
	int32_t ___AlbedoTextureIndex;
	int32_t ___EmissionTextureIndex;
	int32_t ___TransmissionTextureIndex;
	uint32_t ___Flags;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___AlbedoScale;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___AlbedoOffset;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___EmissionScale;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___EmissionOffset;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___TransmissionScale;
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___TransmissionOffset;
	float3_t4AB5D88249ADB24F69FFD0793E8ED25E1CC3745E ___EmissionColor;
	uint32_t ___AlbedoAndEmissionUVChannel;
};
struct NativeRectParams_t18E2E456D50F027D08BF8B4BCBD83DF7FDB1C02F 
{
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___rect;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___subRect;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___uv;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___color;
	int32_t ___scaleMode;
	intptr_t ___backgroundRepeatInstanceList;
	int32_t ___backgroundRepeatInstanceListStartIndex;
	int32_t ___backgroundRepeatInstanceListEndIndex;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topLeftRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomLeftRadius;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___backgroundRepeatRect;
	intptr_t ___texture;
	intptr_t ___sprite;
	intptr_t ___vectorImage;
	intptr_t ___spriteTexture;
	intptr_t ___spriteVertices;
	intptr_t ___spriteUVs;
	intptr_t ___spriteTriangles;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___spriteGeomRect;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___contentSize;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___textureSize;
	float ___texturePixelsPerPoint;
	int32_t ___leftSlice;
	int32_t ___topSlice;
	int32_t ___rightSlice;
	int32_t ___bottomSlice;
	float ___sliceScale;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___rectInset;
	NativeColorPage_tD578D4E96ABBDBB0E6A608F6CE97A0362ABD7D4B ___colorPage;
	int32_t ___meshFlags;
};
struct BorderParams_tA3F0684BE19E681B68F40A773737978EB3FD1509 
{
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___rect;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___playmodeTintColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___leftColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___topColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___rightColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___bottomColor;
	float ___leftWidth;
	float ___topWidth;
	float ___rightWidth;
	float ___bottomWidth;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topLeftRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomLeftRadius;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0 ___leftColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0 ___topColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0 ___rightColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0 ___bottomColorPage;
};
struct BorderParams_tA3F0684BE19E681B68F40A773737978EB3FD1509_marshaled_pinvoke
{
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___rect;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___playmodeTintColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___leftColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___topColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___rightColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___bottomColor;
	float ___leftWidth;
	float ___topWidth;
	float ___rightWidth;
	float ___bottomWidth;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topLeftRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomLeftRadius;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_pinvoke ___leftColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_pinvoke ___topColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_pinvoke ___rightColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_pinvoke ___bottomColorPage;
};
struct BorderParams_tA3F0684BE19E681B68F40A773737978EB3FD1509_marshaled_com
{
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___rect;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___playmodeTintColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___leftColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___topColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___rightColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___bottomColor;
	float ___leftWidth;
	float ___topWidth;
	float ___rightWidth;
	float ___bottomWidth;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topLeftRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___topRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomRightRadius;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___bottomLeftRadius;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_com ___leftColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_com ___topColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_com ___rightColorPage;
	ColorPage_t7C2B8995DE8D27CED5E55F7BFE4E6C70C971FAE0_marshaled_com ___bottomColorPage;
};
struct UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 
{
	NetworkPipeline_t83BB00B4CE9811A2B75D37E50EFB85FD60044A69 ___pipeline;
	int32_t ___stage;
	NetworkConnection_t0A1170D9665C62249582E5DAABC2EAF2D01DEDF4 ___connection;
};
struct InternalQosServer_t56825CF99AB62B0FD12D69A5E5EB72FA463E687A 
{
	NetworkEndPoint_tD248F1CF54F01EF67B92B12963FC7060054C4CBC ___RemoteEndpoint;
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___BackoffUntilUtc;
	int32_t ___Idx;
	int32_t ___m_FirstIdx;
	uint16_t ___m_RequestIdentifier;
};
struct InternalQosServer_t56825CF99AB62B0FD12D69A5E5EB72FA463E687A_marshaled_pinvoke
{
	NetworkEndPoint_tD248F1CF54F01EF67B92B12963FC7060054C4CBC ___RemoteEndpoint;
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___BackoffUntilUtc;
	int32_t ___Idx;
	int32_t ___m_FirstIdx;
	uint16_t ___m_RequestIdentifier;
};
struct InternalQosServer_t56825CF99AB62B0FD12D69A5E5EB72FA463E687A_marshaled_com
{
	NetworkEndPoint_tD248F1CF54F01EF67B92B12963FC7060054C4CBC ___RemoteEndpoint;
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___BackoffUntilUtc;
	int32_t ___Idx;
	int32_t ___m_FirstIdx;
	uint16_t ___m_RequestIdentifier;
};
struct LightDescriptor_tCFFA8945B9D03CA49332A3243AAD453F0EFFD8C5 
{
	int32_t ___Type;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___LinearLightColor;
	int32_t ___Shadows;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___Transform;
	float ___ColorTemperature;
	int32_t ___LightmapBakeType;
	uint8_t ___FalloffType;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___AreaSize;
	float ___SpotAngle;
	float ___InnerSpotAngle;
	uint32_t ___CullingMask;
	float ___BounceIntensity;
	float ___Range;
	int32_t ___ShadowMaskChannel;
	bool ___UseColorTemperature;
	float ___ShadowRadius;
	Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___CookieTexture;
	float ___CookieSize;
};
struct LightDescriptor_tCFFA8945B9D03CA49332A3243AAD453F0EFFD8C5_marshaled_pinvoke
{
	int32_t ___Type;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___LinearLightColor;
	int32_t ___Shadows;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___Transform;
	float ___ColorTemperature;
	int32_t ___LightmapBakeType;
	uint8_t ___FalloffType;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___AreaSize;
	float ___SpotAngle;
	float ___InnerSpotAngle;
	uint32_t ___CullingMask;
	float ___BounceIntensity;
	float ___Range;
	int32_t ___ShadowMaskChannel;
	int32_t ___UseColorTemperature;
	float ___ShadowRadius;
	Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___CookieTexture;
	float ___CookieSize;
};
struct LightDescriptor_tCFFA8945B9D03CA49332A3243AAD453F0EFFD8C5_marshaled_com
{
	int32_t ___Type;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___LinearLightColor;
	int32_t ___Shadows;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___Transform;
	float ___ColorTemperature;
	int32_t ___LightmapBakeType;
	uint8_t ___FalloffType;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___AreaSize;
	float ___SpotAngle;
	float ___InnerSpotAngle;
	uint32_t ___CullingMask;
	float ___BounceIntensity;
	float ___Range;
	int32_t ___ShadowMaskChannel;
	int32_t ___UseColorTemperature;
	float ___ShadowRadius;
	Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___CookieTexture;
	float ___CookieSize;
};
struct ContactBeginTarget_t219EBBED83B122E8CC4DEE88C04DF6F8595882CE 
{
	ContactBeginEvent_t4F38390723F5E35F57A4009B9CACEF4CD212AA90 ___m_BeginEvent;
};
struct ContactEndTarget_t6AFFF294A99330ACB933D8C200E7CA75AEF31B29 
{
	ContactEndEvent_tE811C99069F0369669904C1229780EDB98CCD3AE ___m_EndEvent;
};
struct JointThresholdTarget_tD35F9E05A70D01669DD20FE508E449AF631338FC 
{
	JointThresholdEvent_t4F68DE6F2342237EE679E77E31F30F355EB2D808 ___m_JointThresholdEvent;
};
struct TriggerBeginTarget_t2E0A2F6FBE5E226EE40F7AC2348034FDA9BAB5E7 
{
	TriggerBeginEvent_t377C38E390AB920034AF6E063A969306047BD6B8 ___m_BeginEvent;
};
struct TriggerEndTarget_tE04F3C90BD5BD9EADC98FFFDDA469061A72A3CD9 
{
	TriggerEndEvent_t7FEED3B9C48823CEE1DBB1B83E237B5D485F0FD5 ___m_EndEvent;
};
struct Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F  : public MulticastDelegate_t
{
};
struct Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 
{
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f0;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f1;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f2;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f3;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f4;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f5;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f6;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f7;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f8;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f9;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f10;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f11;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f12;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f13;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f14;
	Array16_1_tD7DC1805EB67B07DF30FB4A25D3C1D0D5C0F9A9F ___f15;
};
struct NativeKeyValueArrays_2_t67E664F1A04EFB50F19C4280E9F48C2DB97FA6AE 
{
	NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588 ___Keys;
	NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588 ___Values;
};
struct NativeKeyValueArrays_2_t6F346645686D9ED3DF9980242A60F6343D9BD346 
{
	NativeArray_1_t2FF0F1BBD323229A2F9ABD52397EA6EF9B105E09 ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_t44905B1CEBDF9B21B174B6B004607DB160A3179F 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_tB7D9C79EBF3AEA35F54C11110B41E68AE0A214F6 ___Values;
};
struct NativeKeyValueArrays_2_t9A441098264B807CEA641B8EA0B3A6099BE560D6 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_t79AF901C035FC0E2DB95A6C857E2A6FE26138AF6 ___Values;
};
struct NativeKeyValueArrays_2_t5819D7BFEF9DAB21CE5F03E645D249BAF749DD29 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_t712BAFB3B7FD80607F953479B6E3EE2D54BD9446 ___Values;
};
struct NativeKeyValueArrays_2_tA2DA312B446D1C4E01DFB5BABCD5FD907A6D98BB 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_t727E0B11B40EA7D6477F67D79DA7B7F7C383735E ___Values;
};
struct NativeKeyValueArrays_2_t69F48B55F7CB9E53D8DA8AC22D78A9FE2C4ACCBB 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_t8E042B4249B3126F27EE49887D2507798DC25F2C ___Values;
};
struct NativeKeyValueArrays_2_tCBCD37F108CC29AF7DD444E7DADEBD5DA915330C 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_tF6C01F5E2A268EC719FE153919FBB3C364489509 
{
	NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___Keys;
	NativeArray_1_tFBC4A05617FFC862FFD574140F343DA82BF818B2 ___Values;
};
struct NativeKeyValueArrays_2_t26BFA27003E5D25D65B6A5407F9D5F228E8740A6 
{
	NativeArray_1_t0215017C25A7FD4BE98E0A8000F004E7D119560D ___Keys;
	NativeArray_1_t1864D78BA2DD264CF10C4F534C4A53B8D45EDFC3 ___Values;
};
struct NativeKeyValueArrays_2_tF51A55D75DBB6D766D8D5B99DC29C3762BA12496 
{
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_t00C5D2C7AF9D9E3622CAB210E6E11E33C4881C8A 
{
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Keys;
	NativeArray_1_t497DD754C21E03BAC4C1F2BB7A3BB0025FF4EB88 ___Values;
};
struct NativeKeyValueArrays_2_t6E475CFF982A098C6F4413A38B259B41803D683C 
{
	NativeArray_1_t25F6CEC65DB0532CB91E2B2890FF6C2D52A210A3 ___Keys;
	NativeArray_1_tD66836534F00AADA4D14B93A8662AF8DA2D65075 ___Values;
};
struct NativeKeyValueArrays_2_tCBEADB7142CDB924EF564F51C98AFFA7C6A162BC 
{
	NativeArray_1_tD5B3D215346B1527FA237140626E9D632A0EE488 ___Keys;
	NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588 ___Values;
};
struct NativeKeyValueArrays_2_t5B99C2646C28A5C04DF011E6E993BDE26A27A953 
{
	NativeArray_1_t8E236FFFE7B598011354300BFCED0A15647BBC5D ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_t2FD6C3D7785A9C84CF994FC08628D01B7947C7C1 
{
	NativeArray_1_tD50C1560C0B57B735333075DF206AB11B0E18565 ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_t31F3CE04B96C7A9192BEB52001190AF4216A7990 
{
	NativeArray_1_t275C00CC374DEA66C69B3BB3992116F315A8E934 ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_t699CBC5BC6A801E2B73A1521AF599E34499857B8 
{
	NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184 ___Keys;
	NativeArray_1_t60C2883F9133642CF381234B3DD254605DD4F0A5 ___Values;
};
struct NativeKeyValueArrays_2_tD4417ED5E6C2DAC8437C9159D79F622F83C72D39 
{
	NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184 ___Keys;
	NativeArray_1_t703E60DA65EBF076EB4916D3B9D9987A5A09B9CC ___Values;
};
struct NativeKeyValueArrays_2_t4AA59D24D1616F48DCDB5CB6C4ABA336C51B1764 
{
	NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184 ___Keys;
	NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___Values;
};
struct NativeKeyValueArrays_2_t97DEBC72840B73CFCEE6195C4C495C675E96FAF1 
{
	NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___Keys;
	NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___Values;
};
struct NativeKeyValueArrays_2_tA9D90BDB6A6A5B59E496BEBBDA8082D2A0598A49 
{
	NativeArray_1_tF0A83538A02306EA7A8F22FEA945A6961365E8C8 ___Keys;
	NativeArray_1_t107C57D0357BCF9956A60495CD8FAADDF1D26AFB ___Values;
};
struct Nullable_1_tEB29AC5A73D1D1AF8DE0D726A51B415DC226387C 
{
	bool ___hasValue;
	NativeArray_1_t0AB49EE6A37F6BC668C15EDFBE9BE92A22B2F0DB ___value;
};
struct Nullable_1_t791F8A662AA857374FA6AFEEEA22B1F1E103327B 
{
	bool ___hasValue;
	NativeArray_1_t5576C5C8F17BB3E1BA11BBA3EC50A55FC1246445 ___value;
};
struct DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 
{
	BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 ___meshID;
	int32_t ___submeshIndex;
	int32_t ___activeMeshLod;
	BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C ___materialID;
	int32_t ___flags;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___transparentInstanceId;
	uint32_t ___overridenComponents;
	RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C ___range;
	int32_t ___lightmapIndex;
};
struct DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_marshaled_pinvoke
{
	BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 ___meshID;
	int32_t ___submeshIndex;
	int32_t ___activeMeshLod;
	BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C ___materialID;
	int32_t ___flags;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___transparentInstanceId;
	uint32_t ___overridenComponents;
	RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_marshaled_pinvoke ___range;
	int32_t ___lightmapIndex;
};
struct DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_marshaled_com
{
	BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 ___meshID;
	int32_t ___submeshIndex;
	int32_t ___activeMeshLod;
	BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C ___materialID;
	int32_t ___flags;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___transparentInstanceId;
	uint32_t ___overridenComponents;
	RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_marshaled_com ___range;
	int32_t ___lightmapIndex;
};
struct DrawingSettings_t3B64EB029DB6F94A1F2A9B2D19D2969AE361BB49 
{
	SortingSettings_t506C3B318FDFD3C2B1620E9B951829C631137E72 ___m_SortingSettings;
	U3CshaderPassNamesU3Ee__FixedBuffer_t5EDC823777BDDC9D50E55FF3779FBC1B3820126D ___shaderPassNames;
	int32_t ___m_PerObjectData;
	int32_t ___m_Flags;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_OverrideShaderID;
	int32_t ___m_OverrideShaderPassIndex;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_OverrideMaterialEntityId;
	int32_t ___m_OverrideMaterialPassIndex;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_fallbackMaterialEntityId;
	int32_t ___m_MainLightIndex;
	int32_t ___m_UseSrpBatcher;
	int32_t ___m_LodCrossFadeStencilMask;
};
struct Long1024_tEE887C506947419DC829213E6C7483D80AF5659F 
{
	Long512_t2D339FF6672EB3709A6C638EABA7D13C7FEC1878 ___f0;
	Long512_t2D339FF6672EB3709A6C638EABA7D13C7FEC1878 ___f1;
};
struct RenderStateBlock_tFC570EF2C8F3A817FECD578E385D18CEEEA06733 
{
	BlendState_tC9B817349E49EF26CBCDC8FCE02789A661DC2630 ___m_BlendState;
	RasterState_tA30E8336EA5D1E2152A6C7252F15384985B98A26 ___m_RasterState;
	DepthState_t798415D2C1D9202E555FEE5D4C5FDF6B3A077255 ___m_DepthState;
	StencilState_tBE5F7C1134E50C5E93B45A626D4FB4690F1C91A9 ___m_StencilState;
	int32_t ___m_StencilReference;
	int32_t ___m_Mask;
};
struct SmallEntityIdArray_t60189E3B3EEF42ED87B04DBA8511AAED039018EE 
{
	FixedList32Bytes_1_t02D797FE0EF11E83016BA95DD7B282490C8A472E ___m_FixedArray;
	UnsafeList_1_t239C74D869DA59B0CA5E63716E71208DEC68A70B ___m_List;
	bool ___m_IsEmbedded;
	bool ___U3CValidU3Ek__BackingField;
	int32_t ___Length;
};
struct SmallEntityIdArray_t60189E3B3EEF42ED87B04DBA8511AAED039018EE_marshaled_pinvoke
{
	FixedList32Bytes_1_t02D797FE0EF11E83016BA95DD7B282490C8A472E ___m_FixedArray;
	UnsafeList_1_t239C74D869DA59B0CA5E63716E71208DEC68A70B ___m_List;
	int32_t ___m_IsEmbedded;
	int32_t ___U3CValidU3Ek__BackingField;
	int32_t ___Length;
};
struct SmallEntityIdArray_t60189E3B3EEF42ED87B04DBA8511AAED039018EE_marshaled_com
{
	FixedList32Bytes_1_t02D797FE0EF11E83016BA95DD7B282490C8A472E ___m_FixedArray;
	UnsafeList_1_t239C74D869DA59B0CA5E63716E71208DEC68A70B ___m_List;
	int32_t ___m_IsEmbedded;
	int32_t ___U3CValidU3Ek__BackingField;
	int32_t ___Length;
};
struct URPLightShadowCullingInfos_t8EBC5966B6C0C703C739850EA3B585324022F0E9 
{
	NativeArray_1_t9B7A94FA050F43A3996B812B9164E7885F38ADC3 ___slices;
	uint32_t ___slicesValidMask;
};
struct Baselib_RegisteredNetwork_Request_t6AF2E70EE04D8C3608FB27091CDE9B4A25A9A4FE 
{
	Baselib_RegisteredNetwork_BufferSlice_tBEE9A8BFFB4625CEE8B4AD97E30DFE397D34F273 ___payload;
	Baselib_RegisteredNetwork_Endpoint_t9A38585FFD3425455A7E0AFC10F16BCD13B1F32C ___remoteEndpoint;
	intptr_t ___requestUserdata;
};
struct HostnameLookupTask_t79592618D9101FF637919ECD8E976DED0C181D9A 
{
	Baselib_NetworkAddress_HostnameLookupHandle_t254C6E9BD97DBB2771BBF6A2F8500DADA2FCE7A8* ___m_LookupHandle;
	FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E ___m_Address;
	int32_t ___RetryCount;
};
struct TessellationJobParameters_tA2407D5C15761590BDD217DDE4861964544E8CE2 
{
	bool ___isBorderJob;
	NativeRectParams_t18E2E456D50F027D08BF8B4BCBD83DF7FDB1C02F ___rectParams;
	BorderParams_tA3F0684BE19E681B68F40A773737978EB3FD1509 ___borderParams;
	UnsafeMeshGenerationNode_t1FF7D38D9B39DC59A2F8E2DFC92BFAB2481E88A2 ___node;
};
struct TessellationJobParameters_tA2407D5C15761590BDD217DDE4861964544E8CE2_marshaled_pinvoke
{
	int32_t ___isBorderJob;
	NativeRectParams_t18E2E456D50F027D08BF8B4BCBD83DF7FDB1C02F ___rectParams;
	BorderParams_tA3F0684BE19E681B68F40A773737978EB3FD1509_marshaled_pinvoke ___borderParams;
	UnsafeMeshGenerationNode_t1FF7D38D9B39DC59A2F8E2DFC92BFAB2481E88A2 ___node;
};
struct TessellationJobParameters_tA2407D5C15761590BDD217DDE4861964544E8CE2_marshaled_com
{
	int32_t ___isBorderJob;
	NativeRectParams_t18E2E456D50F027D08BF8B4BCBD83DF7FDB1C02F ___rectParams;
	BorderParams_tA3F0684BE19E681B68F40A773737978EB3FD1509_marshaled_com ___borderParams;
	UnsafeMeshGenerationNode_t1FF7D38D9B39DC59A2F8E2DFC92BFAB2481E88A2 ___node;
};
struct TransferrableData_tE4092BB2D19869881CCCD6B9B3E54FDC904B3197 
{
	FixedList64Bytes_1_t137BDA0D26652E438404CA31731069295DAC8E1C ___m_RawAddressContainer;
};
struct Painter2DJobData_t227572FEAE4A071ED0378501E752A72FF0ACC4EF 
{
	UnsafeMeshGenerationNode_t1FF7D38D9B39DC59A2F8E2DFC92BFAB2481E88A2 ___node;
	int32_t ___snapshotIndex;
	intptr_t ___vectorImagePtr;
	intptr_t ___texturePtr;
};
struct TransformWriteTween_tA1E06E591FBB1696DE2915757CCA39C6B6E074D0 
{
	PhysicsBody_tDCDAC059BCC8FF8A057AD9B683E04545D3B191A7 ___m_Body;
	int32_t ___m_TransformWriteMode;
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___m_PhysicsTransform;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_LinearVelocity;
	float ___m_AngularVelocity;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_PositionFrom;
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 ___m_RotationFrom;
};
struct BodyUpdateEvent_t82660EC0D1983A12F9407C4977880BD84E5360D9 
{
	intptr_t ___m_UserData;
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___m_Transform;
	PhysicsBody_tDCDAC059BCC8FF8A057AD9B683E04545D3B191A7 ___m_Body;
	bool ___m_FellAsleep;
};
struct BodyUpdateEvent_t82660EC0D1983A12F9407C4977880BD84E5360D9_marshaled_pinvoke
{
	intptr_t ___m_UserData;
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___m_Transform;
	PhysicsBody_tDCDAC059BCC8FF8A057AD9B683E04545D3B191A7 ___m_Body;
	int32_t ___m_FellAsleep;
};
struct BodyUpdateEvent_t82660EC0D1983A12F9407C4977880BD84E5360D9_marshaled_com
{
	intptr_t ___m_UserData;
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___m_Transform;
	PhysicsBody_tDCDAC059BCC8FF8A057AD9B683E04545D3B191A7 ___m_Body;
	int32_t ___m_FellAsleep;
};
struct CompiledPassInfo_t0602472C646D539122A25EDD89C2E65F136A1948 
{
	String_t* ___name;
	int32_t ___index;
	List_1U5BU5D_t37294D7C303231F2FD83B3C398AED0937F4F3206* ___resourceCreateList;
	List_1U5BU5D_t37294D7C303231F2FD83B3C398AED0937F4F3206* ___resourceReleaseList;
	GraphicsFence_t199180163AEDE0C1BE868F8E1314A47610B1FABB ___fence;
	List_1U5BU5D_tE510DA387DA867AC92F8274325B178A7DE9A209E* ___debugResourceReads;
	List_1U5BU5D_tE510DA387DA867AC92F8274325B178A7DE9A209E* ___debugResourceWrites;
	int32_t ___refCount;
	int32_t ___syncToPassIndex;
	int32_t ___syncFromPassIndex;
	bool ___enableAsyncCompute;
	bool ___allowPassCulling;
	bool ___needGraphicsFence;
	bool ___culled;
	bool ___culledByRendererList;
	bool ___hasSideEffect;
	bool ___enableFoveatedRasterization;
	int32_t ___extendedFeatureFlags;
	bool ___hasShadingRateImage;
	bool ___hasShadingRateStates;
};
struct CompiledPassInfo_t0602472C646D539122A25EDD89C2E65F136A1948_marshaled_pinvoke
{
	char* ___name;
	int32_t ___index;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73** ___resourceCreateList;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73** ___resourceReleaseList;
	GraphicsFence_t199180163AEDE0C1BE868F8E1314A47610B1FABB ___fence;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD** ___debugResourceReads;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD** ___debugResourceWrites;
	int32_t ___refCount;
	int32_t ___syncToPassIndex;
	int32_t ___syncFromPassIndex;
	int32_t ___enableAsyncCompute;
	int32_t ___allowPassCulling;
	int32_t ___needGraphicsFence;
	int32_t ___culled;
	int32_t ___culledByRendererList;
	int32_t ___hasSideEffect;
	int32_t ___enableFoveatedRasterization;
	int32_t ___extendedFeatureFlags;
	int32_t ___hasShadingRateImage;
	int32_t ___hasShadingRateStates;
};
struct CompiledPassInfo_t0602472C646D539122A25EDD89C2E65F136A1948_marshaled_com
{
	Il2CppChar* ___name;
	int32_t ___index;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73** ___resourceCreateList;
	List_1_t05915E9237850A58106982B7FE4BC5DA4E872E73** ___resourceReleaseList;
	GraphicsFence_t199180163AEDE0C1BE868F8E1314A47610B1FABB ___fence;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD** ___debugResourceReads;
	List_1_tF470A3BE5C1B5B68E1325EF3F109D172E60BD7CD** ___debugResourceWrites;
	int32_t ___refCount;
	int32_t ___syncToPassIndex;
	int32_t ___syncFromPassIndex;
	int32_t ___enableAsyncCompute;
	int32_t ___allowPassCulling;
	int32_t ___needGraphicsFence;
	int32_t ___culled;
	int32_t ___culledByRendererList;
	int32_t ___hasSideEffect;
	int32_t ___enableFoveatedRasterization;
	int32_t ___extendedFeatureFlags;
	int32_t ___hasShadingRateImage;
	int32_t ___hasShadingRateStates;
};
struct CapsuleGeometryElement_t7283C3F74372B1D8035F99314DCDAFBB9137D164 
{
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___transform;
	float ___radius;
	float ___length;
	float ___elementDepth;
	int32_t ___drawFillOptions;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___color;
};
struct CircleGeometryElement_t49610DEDF104EC781DEA6886ECE01A85F007D4AC 
{
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___transform;
	float ___radius;
	float ___elementDepth;
	int32_t ___drawFillOptions;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___color;
};
struct LineElement_t96DCC25AC17109ACE2D8E83D154E5B7559953339 
{
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___transform;
	float ___length;
	float ___elementDepth;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___color;
};
struct PolygonGeometryElement_t6C8FF21F16A40EA03BE69163226AF7366F22DCFD 
{
	PhysicsTransform_tC034E6903DE863628D032D2452AD049859CE9F6F ___transform;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p0;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p1;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p2;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p3;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p4;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p5;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p6;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___p7;
	int32_t ___count;
	float ___radius;
	float ___elementDepth;
	int32_t ___drawFillOptions;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___color;
};
struct Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 
{
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f0;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f1;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f2;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f3;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f4;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f5;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f6;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f7;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f8;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f9;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f10;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f11;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f12;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f13;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f14;
	Array256_1_t5410033DC9920374048839AA4B80BA4420373D59 ___f15;
};
struct NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC 
{
	TransferrableData_tE4092BB2D19869881CCCD6B9B3E54FDC904B3197 ___Transferrable;
};
struct RendererListParams_t13F72282BCE2DC255747FE7694C6BBC3377944B1 
{
	CullingResults_tD6B7EF20B68D47DFF3A99EB2EA73F47F1D460267 ___cullingResults;
	DrawingSettings_t3B64EB029DB6F94A1F2A9B2D19D2969AE361BB49 ___drawSettings;
	FilteringSettings_t75860B12A7BCF9A0E2F13CB2C2E5DCD9E1EEAD9F ___filteringSettings;
	ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0 ___tagName;
	bool ___isPassTagName;
	Nullable_1_t791F8A662AA857374FA6AFEEEA22B1F1E103327B ___tagValues;
	Nullable_1_tEB29AC5A73D1D1AF8DE0D726A51B415DC226387C ___stateBlocks;
};
struct RendererListParams_t13F72282BCE2DC255747FE7694C6BBC3377944B1_marshaled_pinvoke
{
	CullingResults_tD6B7EF20B68D47DFF3A99EB2EA73F47F1D460267 ___cullingResults;
	DrawingSettings_t3B64EB029DB6F94A1F2A9B2D19D2969AE361BB49 ___drawSettings;
	FilteringSettings_t75860B12A7BCF9A0E2F13CB2C2E5DCD9E1EEAD9F ___filteringSettings;
	ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0 ___tagName;
	int32_t ___isPassTagName;
	Nullable_1_t791F8A662AA857374FA6AFEEEA22B1F1E103327B ___tagValues;
	Nullable_1_tEB29AC5A73D1D1AF8DE0D726A51B415DC226387C ___stateBlocks;
};
struct RendererListParams_t13F72282BCE2DC255747FE7694C6BBC3377944B1_marshaled_com
{
	CullingResults_tD6B7EF20B68D47DFF3A99EB2EA73F47F1D460267 ___cullingResults;
	DrawingSettings_t3B64EB029DB6F94A1F2A9B2D19D2969AE361BB49 ___drawSettings;
	FilteringSettings_t75860B12A7BCF9A0E2F13CB2C2E5DCD9E1EEAD9F ___filteringSettings;
	ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0 ___tagName;
	int32_t ___isPassTagName;
	Nullable_1_t791F8A662AA857374FA6AFEEEA22B1F1E103327B ___tagValues;
	Nullable_1_tEB29AC5A73D1D1AF8DE0D726A51B415DC226387C ___stateBlocks;
};
struct HostnameLookupData_t1B68029C2268ABA3B91249AEEA15B0E4387ACB1F 
{
	HostnameLookupTask_t79592618D9101FF637919ECD8E976DED0C181D9A ___Task;
	uint16_t ___Port;
};
struct BodyUpdateTarget_t6DFEB7E2EB41937A4D139AAA98A0B2FEF61145DC 
{
	BodyUpdateEvent_t82660EC0D1983A12F9407C4977880BD84E5360D9 ___m_BodyUpdateEvent;
};
struct BodyUpdateTarget_t6DFEB7E2EB41937A4D139AAA98A0B2FEF61145DC_marshaled_pinvoke
{
	BodyUpdateEvent_t82660EC0D1983A12F9407C4977880BD84E5360D9_marshaled_pinvoke ___m_BodyUpdateEvent;
};
struct BodyUpdateTarget_t6DFEB7E2EB41937A4D139AAA98A0B2FEF61145DC_marshaled_com
{
	BodyUpdateEvent_t82660EC0D1983A12F9407C4977880BD84E5360D9_marshaled_com ___m_BodyUpdateEvent;
};
struct Array32768_1_tF94DB9E949B98E267CCEE7E61378AA0A89C951D6 
{
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f0;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f1;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f2;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f3;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f4;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f5;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f6;
	Array4096_1_t97465F61B5971FE36BADFE9A629DEB33EF47E8D8 ___f7;
};
struct RendererListResource_tCB75EF1874F8A294101A45F937987CC314B92214 
{
	RendererListParams_t13F72282BCE2DC255747FE7694C6BBC3377944B1 ___desc;
	RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 ___rendererList;
};
struct RendererListResource_tCB75EF1874F8A294101A45F937987CC314B92214_marshaled_pinvoke
{
	RendererListParams_t13F72282BCE2DC255747FE7694C6BBC3377944B1_marshaled_pinvoke ___desc;
	RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 ___rendererList;
};
struct RendererListResource_tCB75EF1874F8A294101A45F937987CC314B92214_marshaled_com
{
	RendererListParams_t13F72282BCE2DC255747FE7694C6BBC3377944B1_marshaled_com ___desc;
	RendererList_t608CE60421616EF4211F5B8AC62E3C36D4BDDF85 ___rendererList;
};
struct PendingPacket_t479815078D1E4D1B5441E44C39E685232AB4EE94 
{
	U3CPacketU3Ee__FixedBuffer_t55E02F8104D901D9FE76233288015124810472C7 ___Packet;
	ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB ___ConnectionId;
	NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC ___Endpoint;
	int64_t ___PendingUntil;
	int32_t ___Length;
	int32_t ___Offset;
};
struct List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4_StaticFields
{
	__Il2CppFullySharedGenericStructTypeU5BU5D_tF3B929B6E80D0A8C109178E11CE8FF9957B014C1* ___s_emptyArray;
};
struct BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_StaticFields
{
	BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 ___Null;
};
struct BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_StaticFields
{
	BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C ___Null;
};
struct BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_StaticFields
{
	BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 ___Null;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_StaticFields
{
	ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB ___U3CInvalidU3Ek__BackingField;
};
struct GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_StaticFields
{
	GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 ___Invalid;
};
struct GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_StaticFields
{
	GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 ___Invalid;
};
struct InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_StaticFields
{
	InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B ___Invalid;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_StaticFields
{
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 ___identityQuaternion;
};
struct Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_StaticFields
{
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___kZero;
};
struct ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0_StaticFields
{
	ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0 ___none;
};
struct SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_StaticFields
{
	SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 ___Invalid;
};
struct Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7_StaticFields
{
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___zeroVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___oneVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___upVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___downVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___leftVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___rightVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___positiveInfinityVector;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___negativeInfinityVector;
};
struct Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A_StaticFields
{
	Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A ___s_Zero;
	Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A ___s_One;
	Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A ___s_Up;
	Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A ___s_Down;
	Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A ___s_Left;
	Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A ___s_Right;
};
struct Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_StaticFields
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___zeroVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___oneVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___upVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___downVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___leftVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___rightVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___forwardVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___backVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___positiveInfinityVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___negativeInfinityVector;
};
struct Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376_StaticFields
{
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Zero;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_One;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Up;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Down;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Left;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Right;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Forward;
	Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376 ___s_Back;
};
struct Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3_StaticFields
{
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___zeroVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___oneVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___positiveInfinityVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___negativeInfinityVector;
};
struct float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA_StaticFields
{
	float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA ___zero;
};
struct float3_t4AB5D88249ADB24F69FFD0793E8ED25E1CC3745E_StaticFields
{
	float3_t4AB5D88249ADB24F69FFD0793E8ED25E1CC3745E ___zero;
};
struct float4_t89D9A294E7A79BD81BFBDD18654508532958555E_StaticFields
{
	float4_t89D9A294E7A79BD81BFBDD18654508532958555E ___zero;
};
struct Block_tBD2149E220F819A66A4A0A18F237932ACD9011DE_StaticFields
{
	Block_tBD2149E220F819A66A4A0A18F237932ACD9011DE ___Invalid;
};
struct GeoPoolEntrySlot_tE1B7B103A90E4C3379CED82C8D6DA55189422977_StaticFields
{
	GeoPoolEntrySlot_tE1B7B103A90E4C3379CED82C8D6DA55189422977 ___Invalid;
};
struct CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7_StaticFields
{
	bool ___ThrowOnSetRenderTarget;
};
struct ShadowSplitData_tC276A96F461DD73CFF6D94DB557D42A1643640DF_StaticFields
{
	int32_t ___maximumCullingPlaneCount;
};
struct StreamCompressionModel_t7F9FB7DB2D88DA832A4DD61C1E6587A4C530697D_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___k_BucketSizes;
	UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___k_BucketOffsets;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___k_FirstBucketCandidate;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___k_DefaultModelData;
};
struct Vertex_t016AC68A2E6C62576E65412BEC71544AFC01AFC7_StaticFields
{
	float ___nearZ;
};
struct float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2_StaticFields
{
	float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2 ___identity;
	float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2 ___zero;
};
struct quaternion_tD6BCBECAF088B9EBAE2345EC8534C7A1A4C910D4_StaticFields
{
	quaternion_tD6BCBECAF088B9EBAE2345EC8534C7A1A4C910D4 ___identity;
};
struct RenderTargetIdentifier_tA528663AC6EB3911D8E91AA40F7070FA5455442B_StaticFields
{
	RenderTargetIdentifier_tA528663AC6EB3911D8E91AA40F7070FA5455442B ___Invalid;
};
struct ResourceHandle_tD3B1FFBD59EB9C23F0A020351836F834C4BD276C_StaticFields
{
	uint32_t ___s_CurrentValidBit;
	uint32_t ___s_SharedResourceValidBit;
};
struct StoreAudit_t9E8FF144788FDFF9C68E912C5BB87C533F860292_StaticFields
{
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___StoreReasonMessages;
};
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700_StaticFields
{
	int32_t ___GenerateAllMips;
};
struct TileData_tFB814629D010ABD175127C0BE96FD96EA606E00F_StaticFields
{
	TileData_tFB814629D010ABD175127C0BE96FD96EA606E00F ___Default;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif


IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t UnsafeUtility_SizeOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m97C7D5E5DE74DC60A0ECAA914830BEDF2C46ACAA_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UnsafeUtility_AlignOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mB152D3C658FB48DEDB6E662EEB0C91018E7F2857_gshared (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void UnsafeUtility_CopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mD5A26D4923C66071413642D33867560D087F543F_gshared_inline (uint8_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m77D1ED5AB6CFA9B6FF9CC1CFB2B72783A7828BF5_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, uint8_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t UnsafeUtility_SizeOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mED481D505BF43CBD96972069EDD4E3509BE84931_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UnsafeUtility_AlignOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m274F833CD001C63E56A22D8F42C76CE8C6CC39DF_gshared (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void UnsafeUtility_CopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m934C9E3CBBFA04ABEF4528E565C300B5DCF8C2D1_gshared_inline (int32_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mDDC9C1D1487BB5B9B776AD9F9B6581F838DA5C09_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, Il2CppFullySharedGenericStruct ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mBC28FDDA775CF46EBFF35FACA19AB97DA55D7C5C_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_AddArray_TisIl2CppFullySharedGenericStruct_m5627B5F2DCE95940F3F9DFF77A97789E2CCD5E57_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, void* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Pop_TisIl2CppFullySharedGenericStruct_m504B53F43031CB15FF39BBBA94BE052B3E33D1A9_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, Il2CppFullySharedGenericStruct* il2cppRetVal, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mCBA4B05BA324F08F46C24FAEAF26B6FA71706FDC_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEA2D219466CA70D0772888D0B2AA5BDF95B3D7B0_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m618DEF61FF37029CC5BB378A9DF74D17BF431F06_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m1FF8E4DB8A087F7A862C15EEC0913C3FCD81FFF5_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m4F68FC2BD5F10999A62EE62207C4B8B4E57489D1_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mBBBCD7A3795516F652E2A209921D9F365FEF117D_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mFCFCB281237C3FD181B82326F1CDC74ECDC26835_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m28DB190FC25AC6811E2F609C118B25148772E8ED_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mB677466F5AF7999905E08C12DA5F85B135C65DA2_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mA6587978CB654E5F829A8E1413F71B7210CE52FB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB151D38E73EEDFB7B82F1F6498C9906EB81D402A_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m7CF1071B4128F92A069498179CE85F80CB5AEBE1_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mDDA0397F7F044E6209E9F8A2D66077E9E5CB485E_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m8BB02CE71160A2514C280DBDC8A5253F93E3689E_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m47E7EA1FA34E44C4B7E215FA109EFC5813E93948_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE5DE2AA98BC9864F8C516BE7C30A490B373C35D7_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m20514B296368E3BD19C54B8257A916FCC7837FCB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mF58A503E41D28232BA45EF49B867720056B3C3EB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m84BBCADD738D8E17724D100724F1173F24CC9D46_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m92740FC50E0692378ABED6D572FE0AA5FFF18DFB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m350360D9B128D3CC31EB85A15C4801B0D0CBDED2_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int64_t UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint16_t UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint32_t UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 UnsafeUtility_ReadArrayElement_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_mC1917A7646F09213727BC23D1069EE21D2BF3920_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m5240736CF4A860AEF91AB40A56C0FEA6010842F3_gshared (NativeKeyValueArrays_2_t67E664F1A04EFB50F19C4280E9F48C2DB97FA6AE* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m4BCBCB4D1AC60559B41D33E7097084B82F700097_gshared (NativeKeyValueArrays_2_t6F346645686D9ED3DF9980242A60F6343D9BD346* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_mDC9713B737CE15E2D2763FB40645AF1983C27BD5_gshared (NativeKeyValueArrays_2_t44905B1CEBDF9B21B174B6B004607DB160A3179F* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C UnsafeUtility_ReadArrayElement_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mDE41FFE3160D153E47D8D93EF29E4B9C3DB23D20_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_mCE5C2D5A8F53DEC83DF1BE432C8EA289E3F41CEF_gshared (NativeKeyValueArrays_2_t9A441098264B807CEA641B8EA0B3A6099BE560D6* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 UnsafeUtility_ReadArrayElement_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mD0CAE37D791EB711A170B7F9BDCEAB6ED472C3E3_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_mB7698B5AC6930259C71BDD34DE4AF096F07CED38_gshared (NativeKeyValueArrays_2_t5819D7BFEF9DAB21CE5F03E645D249BAF749DD29* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 UnsafeUtility_ReadArrayElement_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_mBA0DB77712EA2A549142CA67C7AE16B595A60205_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m686910AE937E76C8D8BAC2FA971F98F55668D1A1_gshared (NativeKeyValueArrays_2_tA2DA312B446D1C4E01DFB5BABCD5FD907A6D98BB* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 UnsafeUtility_ReadArrayElement_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF42F014B5AA9C633D3FCAB3FFF26646E54395BD8_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_mFCC843E8A6076977EDC711B85D5A812409F96A92_gshared (NativeKeyValueArrays_2_t69F48B55F7CB9E53D8DA8AC22D78A9FE2C4ACCBB* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B UnsafeUtility_ReadArrayElement_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m86FC76E325E9D3B56133BB7A193BAF12B47FBCC1_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m8AD3EFA2ADD479EB790D599C9BF297BBFE6A6E22_gshared (NativeKeyValueArrays_2_tCBCD37F108CC29AF7DD444E7DADEBD5DA915330C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m231B05B8DA777B05DDB21032416A0EFAAA7770D1_gshared (NativeKeyValueArrays_2_tF6C01F5E2A268EC719FE153919FBB3C364489509* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E UnsafeUtility_ReadArrayElement_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m6992824D0CDD449EFB81329A07FAC0AC09F03218_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m7409414C2CB2F8B001EDB340C888A267B2668251_gshared (NativeKeyValueArrays_2_t26BFA27003E5D25D65B6A5407F9D5F228E8740A6* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E UnsafeUtility_ReadArrayElement_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m2ADCB1A0DE89B2E013B4BB7D300F81AECACFE8EA_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m0E87F558427CED97F501D4AB4A5F0A78A2B39723_gshared (NativeKeyValueArrays_2_tF51A55D75DBB6D766D8D5B99DC29C3762BA12496* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m02951443FBCF5B78507CC0C916DA7B406A6E2FFC_gshared (NativeKeyValueArrays_2_t00C5D2C7AF9D9E3622CAB210E6E11E33C4881C8A* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 UnsafeUtility_ReadArrayElement_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m8460969BD9998B0DFF865186874B2414FEED12C8_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m9C73B4588FF12C4843A4CA6E3548C69471837599_gshared (NativeKeyValueArrays_2_t6E475CFF982A098C6F4413A38B259B41803D683C* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 UnsafeUtility_ReadArrayElement_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m14BBB3C75525EE04E460C0A3D179C85C5BC7C612_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_mB5AA196744005B710F1B67E470ADA3C78AAA8E84_gshared (NativeKeyValueArrays_2_tCBEADB7142CDB924EF564F51C98AFFA7C6A162BC* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m354A6BD5F31BC54C9F3D2B8A51EF1C71C9005648_gshared (NativeKeyValueArrays_2_t5B99C2646C28A5C04DF011E6E993BDE26A27A953* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m9FF445C8935DB92A9DB6A61D0428F6E1301EA17A_gshared (NativeKeyValueArrays_2_t2FD6C3D7785A9C84CF994FC08628D01B7947C7C1* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m5D960A4C192B677E942F8EE6033822A69C5F22F8_gshared (NativeKeyValueArrays_2_t31F3CE04B96C7A9192BEB52001190AF4216A7990* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m08640C8B3F96CF59CE7C409ED1E5A53510CE219F_gshared (NativeKeyValueArrays_2_t699CBC5BC6A801E2B73A1521AF599E34499857B8* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 UnsafeUtility_ReadArrayElement_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m80C61348EC8F9D4F73F0AAA4A69A028E6D599DCD_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m0DD220DB30C031B6200A4119D0DCF56B565846E6_gshared (NativeKeyValueArrays_2_tD4417ED5E6C2DAC8437C9159D79F622F83C72D39* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 UnsafeUtility_ReadArrayElement_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m2A66DBE83F9498AC60B42C9A0B1C7E4401B4CDCC_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m8C99316414D47C54EF1AFF05D069266C1FE65840_gshared (NativeKeyValueArrays_2_t4AA59D24D1616F48DCDB5CB6C4ABA336C51B1764* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NativeKeyValueArrays_2_get_Length_m4A745207D9267220A0F145A1E29332EF7D212017_gshared (NativeKeyValueArrays_2_tA9D90BDB6A6A5B59E496BEBBDA8082D2A0598A49* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool UnsafeUtility_ReadArrayElement_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mF765CB2E5FD631DFB79C25DE656F3C1BB359B995_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_gshared (uint8_t* ___0_pointer, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___1_allocator, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeUtility_InternalCopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mFB7A3FF45C4521DC402828AC45C2220BC22ACF0A_gshared (uint8_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeUtility_InternalCopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m305D0FDE533B9169C6E61EDFEC9CFB0564E7B14B_gshared (int32_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method) ;

inline int32_t UnsafeUtility_SizeOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m97C7D5E5DE74DC60A0ECAA914830BEDF2C46ACAA_inline (const RuntimeMethod* method)
{
	return ((  int32_t (*) (const RuntimeMethod*))UnsafeUtility_SizeOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m97C7D5E5DE74DC60A0ECAA914830BEDF2C46ACAA_gshared_inline)(method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_SetCapacity_mF80BF3DD388E819E52586A458C10606333611269 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, int32_t ___0_capacity, const RuntimeMethod* method) ;
inline int32_t UnsafeUtility_AlignOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mB152D3C658FB48DEDB6E662EEB0C91018E7F2857 (const RuntimeMethod* method)
{
	return ((  int32_t (*) (const RuntimeMethod*))UnsafeUtility_AlignOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mB152D3C658FB48DEDB6E662EEB0C91018E7F2857_gshared)(method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool CollectionHelper_IsAligned_m4761669B9084B892256349C0FF27DBF494DA9AE9 (uint64_t ___0_offset, int32_t ___1_alignmentPowerOfTwo, const RuntimeMethod* method) ;
inline void UnsafeUtility_CopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mD5A26D4923C66071413642D33867560D087F543F_inline (uint8_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method)
{
	((  void (*) (uint8_t*, void*, const RuntimeMethod*))UnsafeUtility_CopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mD5A26D4923C66071413642D33867560D087F543F_gshared_inline)(___0_input, ___1_ptr, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177 (void* ___0_destination, void* ___1_source, int64_t ___2_size, const RuntimeMethod* method) ;
inline void UnsafeAppendBuffer_Add_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m77D1ED5AB6CFA9B6FF9CC1CFB2B72783A7828BF5 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, uint8_t ___0_value, const RuntimeMethod* method)
{
	((  void (*) (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*, uint8_t, const RuntimeMethod*))UnsafeAppendBuffer_Add_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m77D1ED5AB6CFA9B6FF9CC1CFB2B72783A7828BF5_gshared)(__this, ___0_value, method);
}
inline int32_t UnsafeUtility_SizeOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mED481D505BF43CBD96972069EDD4E3509BE84931_inline (const RuntimeMethod* method)
{
	return ((  int32_t (*) (const RuntimeMethod*))UnsafeUtility_SizeOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mED481D505BF43CBD96972069EDD4E3509BE84931_gshared_inline)(method);
}
inline int32_t UnsafeUtility_AlignOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m274F833CD001C63E56A22D8F42C76CE8C6CC39DF (const RuntimeMethod* method)
{
	return ((  int32_t (*) (const RuntimeMethod*))UnsafeUtility_AlignOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m274F833CD001C63E56A22D8F42C76CE8C6CC39DF_gshared)(method);
}
inline void UnsafeUtility_CopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m934C9E3CBBFA04ABEF4528E565C300B5DCF8C2D1_inline (int32_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method)
{
	((  void (*) (int32_t*, void*, const RuntimeMethod*))UnsafeUtility_CopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m934C9E3CBBFA04ABEF4528E565C300B5DCF8C2D1_gshared_inline)(___0_input, ___1_ptr, method);
}
inline void UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, int32_t ___0_value, const RuntimeMethod* method)
{
	((  void (*) (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*, int32_t, const RuntimeMethod*))UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_gshared)(__this, ___0_value, method);
}
inline void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mDDC9C1D1487BB5B9B776AD9F9B6581F838DA5C09 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, Il2CppFullySharedGenericStruct ___0_value, const RuntimeMethod* method)
{
	((  void (*) (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*, Il2CppFullySharedGenericStruct, const RuntimeMethod*))UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mDDC9C1D1487BB5B9B776AD9F9B6581F838DA5C09_gshared)((UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*)__this, ___0_value, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_m1CE98EFF1C7CAFEDCDE896B21530D3B2A3311D25 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, void* ___0_ptr, int32_t ___1_structSize, const RuntimeMethod* method) ;
inline void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mBC28FDDA775CF46EBFF35FACA19AB97DA55D7C5C (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___0_value, const RuntimeMethod* method)
{
	((  void (*) (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, const RuntimeMethod*))UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mBC28FDDA775CF46EBFF35FACA19AB97DA55D7C5C_gshared)(__this, ___0_value, method);
}
inline void UnsafeAppendBuffer_AddArray_TisIl2CppFullySharedGenericStruct_m5627B5F2DCE95940F3F9DFF77A97789E2CCD5E57 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, void* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*, void*, int32_t, const RuntimeMethod*))UnsafeAppendBuffer_AddArray_TisIl2CppFullySharedGenericStruct_m5627B5F2DCE95940F3F9DFF77A97789E2CCD5E57_gshared)(__this, ___0_ptr, ___1_length, method);
}
inline void UnsafeAppendBuffer_Pop_TisIl2CppFullySharedGenericStruct_m504B53F43031CB15FF39BBBA94BE052B3E33D1A9 (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, Il2CppFullySharedGenericStruct* il2cppRetVal, const RuntimeMethod* method)
{
	((  void (*) (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*, Il2CppFullySharedGenericStruct*, const RuntimeMethod*))UnsafeAppendBuffer_Pop_TisIl2CppFullySharedGenericStruct_m504B53F43031CB15FF39BBBA94BE052B3E33D1A9_gshared)((UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*)__this, il2cppRetVal, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663 (bool ___0_condition, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void* Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5 (int64_t ___0_size, int32_t ___1_align, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_allocator, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int64_t math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline (int64_t ___0_x, const RuntimeMethod* method) ;
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mCBA4B05BA324F08F46C24FAEAF26B6FA71706FDC (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mCBA4B05BA324F08F46C24FAEAF26B6FA71706FDC_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEA2D219466CA70D0772888D0B2AA5BDF95B3D7B0 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEA2D219466CA70D0772888D0B2AA5BDF95B3D7B0_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m618DEF61FF37029CC5BB378A9DF74D17BF431F06 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m618DEF61FF37029CC5BB378A9DF74D17BF431F06_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m1FF8E4DB8A087F7A862C15EEC0913C3FCD81FFF5 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m1FF8E4DB8A087F7A862C15EEC0913C3FCD81FFF5_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m4F68FC2BD5F10999A62EE62207C4B8B4E57489D1 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m4F68FC2BD5F10999A62EE62207C4B8B4E57489D1_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mBBBCD7A3795516F652E2A209921D9F365FEF117D (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mBBBCD7A3795516F652E2A209921D9F365FEF117D_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mFCFCB281237C3FD181B82326F1CDC74ECDC26835 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mFCFCB281237C3FD181B82326F1CDC74ECDC26835_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m28DB190FC25AC6811E2F609C118B25148772E8ED (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m28DB190FC25AC6811E2F609C118B25148772E8ED_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mB677466F5AF7999905E08C12DA5F85B135C65DA2 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mB677466F5AF7999905E08C12DA5F85B135C65DA2_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mA6587978CB654E5F829A8E1413F71B7210CE52FB (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mA6587978CB654E5F829A8E1413F71B7210CE52FB_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB151D38E73EEDFB7B82F1F6498C9906EB81D402A (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB151D38E73EEDFB7B82F1F6498C9906EB81D402A_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m7CF1071B4128F92A069498179CE85F80CB5AEBE1 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m7CF1071B4128F92A069498179CE85F80CB5AEBE1_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mDDA0397F7F044E6209E9F8A2D66077E9E5CB485E (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mDDA0397F7F044E6209E9F8A2D66077E9E5CB485E_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m8BB02CE71160A2514C280DBDC8A5253F93E3689E (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m8BB02CE71160A2514C280DBDC8A5253F93E3689E_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m47E7EA1FA34E44C4B7E215FA109EFC5813E93948 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m47E7EA1FA34E44C4B7E215FA109EFC5813E93948_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE5DE2AA98BC9864F8C516BE7C30A490B373C35D7 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE5DE2AA98BC9864F8C516BE7C30A490B373C35D7_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m20514B296368E3BD19C54B8257A916FCC7837FCB (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m20514B296368E3BD19C54B8257A916FCC7837FCB_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mF58A503E41D28232BA45EF49B867720056B3C3EB (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mF58A503E41D28232BA45EF49B867720056B3C3EB_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m84BBCADD738D8E17724D100724F1173F24CC9D46 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m84BBCADD738D8E17724D100724F1173F24CC9D46_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m92740FC50E0692378ABED6D572FE0AA5FFF18DFB (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m92740FC50E0692378ABED6D572FE0AA5FFF18DFB_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
inline int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m350360D9B128D3CC31EB85A15C4801B0D0CBDED2 (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method)
{
	return ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))UnsafeParallelHashMapData_CalculateDataSize_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m350360D9B128D3CC31EB85A15C4801B0D0CBDED2_gshared)(___0_length, ___1_bucketLength, ___2_keyOffset, ___3_nextOffset, ___4_bucketOffset, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82 (int64_t ___0_size, int32_t ___1_alignmentPowerOfTwo, const RuntimeMethod* method) ;
inline ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_gshared_inline)(___0_source, ___1_index, method);
}
inline DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_gshared_inline)(___0_source, ___1_index, method);
}
inline EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_gshared_inline)(___0_source, ___1_index, method);
}
inline FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  int32_t (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_gshared_inline)(___0_source, ___1_index, method);
}
inline int64_t UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  int64_t (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_gshared_inline)(___0_source, ___1_index, method);
}
inline NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_gshared_inline)(___0_source, ___1_index, method);
}
inline RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_gshared_inline)(___0_source, ___1_index, method);
}
inline SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_gshared_inline)(___0_source, ___1_index, method);
}
inline uint16_t UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  uint16_t (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_gshared_inline)(___0_source, ___1_index, method);
}
inline uint32_t UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  uint32_t (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_gshared_inline)(___0_source, ___1_index, method);
}
inline UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 UnsafeUtility_ReadArrayElement_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_mC1917A7646F09213727BC23D1069EE21D2BF3920_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_mC1917A7646F09213727BC23D1069EE21D2BF3920_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m5240736CF4A860AEF91AB40A56C0FEA6010842F3 (NativeKeyValueArrays_2_t67E664F1A04EFB50F19C4280E9F48C2DB97FA6AE* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t67E664F1A04EFB50F19C4280E9F48C2DB97FA6AE*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m5240736CF4A860AEF91AB40A56C0FEA6010842F3_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m4BCBCB4D1AC60559B41D33E7097084B82F700097 (NativeKeyValueArrays_2_t6F346645686D9ED3DF9980242A60F6343D9BD346* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t6F346645686D9ED3DF9980242A60F6343D9BD346*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m4BCBCB4D1AC60559B41D33E7097084B82F700097_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_mDC9713B737CE15E2D2763FB40645AF1983C27BD5 (NativeKeyValueArrays_2_t44905B1CEBDF9B21B174B6B004607DB160A3179F* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t44905B1CEBDF9B21B174B6B004607DB160A3179F*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_mDC9713B737CE15E2D2763FB40645AF1983C27BD5_gshared)(__this, method);
}
inline BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C UnsafeUtility_ReadArrayElement_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mDE41FFE3160D153E47D8D93EF29E4B9C3DB23D20_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mDE41FFE3160D153E47D8D93EF29E4B9C3DB23D20_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_mCE5C2D5A8F53DEC83DF1BE432C8EA289E3F41CEF (NativeKeyValueArrays_2_t9A441098264B807CEA641B8EA0B3A6099BE560D6* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t9A441098264B807CEA641B8EA0B3A6099BE560D6*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_mCE5C2D5A8F53DEC83DF1BE432C8EA289E3F41CEF_gshared)(__this, method);
}
inline BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 UnsafeUtility_ReadArrayElement_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mD0CAE37D791EB711A170B7F9BDCEAB6ED472C3E3_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mD0CAE37D791EB711A170B7F9BDCEAB6ED472C3E3_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_mB7698B5AC6930259C71BDD34DE4AF096F07CED38 (NativeKeyValueArrays_2_t5819D7BFEF9DAB21CE5F03E645D249BAF749DD29* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t5819D7BFEF9DAB21CE5F03E645D249BAF749DD29*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_mB7698B5AC6930259C71BDD34DE4AF096F07CED38_gshared)(__this, method);
}
inline GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 UnsafeUtility_ReadArrayElement_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_mBA0DB77712EA2A549142CA67C7AE16B595A60205_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_mBA0DB77712EA2A549142CA67C7AE16B595A60205_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m686910AE937E76C8D8BAC2FA971F98F55668D1A1 (NativeKeyValueArrays_2_tA2DA312B446D1C4E01DFB5BABCD5FD907A6D98BB* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tA2DA312B446D1C4E01DFB5BABCD5FD907A6D98BB*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m686910AE937E76C8D8BAC2FA971F98F55668D1A1_gshared)(__this, method);
}
inline GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 UnsafeUtility_ReadArrayElement_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF42F014B5AA9C633D3FCAB3FFF26646E54395BD8_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF42F014B5AA9C633D3FCAB3FFF26646E54395BD8_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_mFCC843E8A6076977EDC711B85D5A812409F96A92 (NativeKeyValueArrays_2_t69F48B55F7CB9E53D8DA8AC22D78A9FE2C4ACCBB* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t69F48B55F7CB9E53D8DA8AC22D78A9FE2C4ACCBB*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_mFCC843E8A6076977EDC711B85D5A812409F96A92_gshared)(__this, method);
}
inline InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B UnsafeUtility_ReadArrayElement_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m86FC76E325E9D3B56133BB7A193BAF12B47FBCC1_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m86FC76E325E9D3B56133BB7A193BAF12B47FBCC1_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m8AD3EFA2ADD479EB790D599C9BF297BBFE6A6E22 (NativeKeyValueArrays_2_tCBCD37F108CC29AF7DD444E7DADEBD5DA915330C* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tCBCD37F108CC29AF7DD444E7DADEBD5DA915330C*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m8AD3EFA2ADD479EB790D599C9BF297BBFE6A6E22_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m231B05B8DA777B05DDB21032416A0EFAAA7770D1 (NativeKeyValueArrays_2_tF6C01F5E2A268EC719FE153919FBB3C364489509* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tF6C01F5E2A268EC719FE153919FBB3C364489509*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m231B05B8DA777B05DDB21032416A0EFAAA7770D1_gshared)(__this, method);
}
inline PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E UnsafeUtility_ReadArrayElement_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m6992824D0CDD449EFB81329A07FAC0AC09F03218_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m6992824D0CDD449EFB81329A07FAC0AC09F03218_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m7409414C2CB2F8B001EDB340C888A267B2668251 (NativeKeyValueArrays_2_t26BFA27003E5D25D65B6A5407F9D5F228E8740A6* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t26BFA27003E5D25D65B6A5407F9D5F228E8740A6*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m7409414C2CB2F8B001EDB340C888A267B2668251_gshared)(__this, method);
}
inline FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E UnsafeUtility_ReadArrayElement_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m2ADCB1A0DE89B2E013B4BB7D300F81AECACFE8EA_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m2ADCB1A0DE89B2E013B4BB7D300F81AECACFE8EA_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m0E87F558427CED97F501D4AB4A5F0A78A2B39723 (NativeKeyValueArrays_2_tF51A55D75DBB6D766D8D5B99DC29C3762BA12496* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tF51A55D75DBB6D766D8D5B99DC29C3762BA12496*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m0E87F558427CED97F501D4AB4A5F0A78A2B39723_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m02951443FBCF5B78507CC0C916DA7B406A6E2FFC (NativeKeyValueArrays_2_t00C5D2C7AF9D9E3622CAB210E6E11E33C4881C8A* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t00C5D2C7AF9D9E3622CAB210E6E11E33C4881C8A*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m02951443FBCF5B78507CC0C916DA7B406A6E2FFC_gshared)(__this, method);
}
inline AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 UnsafeUtility_ReadArrayElement_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m8460969BD9998B0DFF865186874B2414FEED12C8_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m8460969BD9998B0DFF865186874B2414FEED12C8_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m9C73B4588FF12C4843A4CA6E3548C69471837599 (NativeKeyValueArrays_2_t6E475CFF982A098C6F4413A38B259B41803D683C* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t6E475CFF982A098C6F4413A38B259B41803D683C*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m9C73B4588FF12C4843A4CA6E3548C69471837599_gshared)(__this, method);
}
inline ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 UnsafeUtility_ReadArrayElement_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m14BBB3C75525EE04E460C0A3D179C85C5BC7C612_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m14BBB3C75525EE04E460C0A3D179C85C5BC7C612_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_mB5AA196744005B710F1B67E470ADA3C78AAA8E84 (NativeKeyValueArrays_2_tCBEADB7142CDB924EF564F51C98AFFA7C6A162BC* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tCBEADB7142CDB924EF564F51C98AFFA7C6A162BC*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_mB5AA196744005B710F1B67E470ADA3C78AAA8E84_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m354A6BD5F31BC54C9F3D2B8A51EF1C71C9005648 (NativeKeyValueArrays_2_t5B99C2646C28A5C04DF011E6E993BDE26A27A953* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t5B99C2646C28A5C04DF011E6E993BDE26A27A953*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m354A6BD5F31BC54C9F3D2B8A51EF1C71C9005648_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m9FF445C8935DB92A9DB6A61D0428F6E1301EA17A (NativeKeyValueArrays_2_t2FD6C3D7785A9C84CF994FC08628D01B7947C7C1* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t2FD6C3D7785A9C84CF994FC08628D01B7947C7C1*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m9FF445C8935DB92A9DB6A61D0428F6E1301EA17A_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m5D960A4C192B677E942F8EE6033822A69C5F22F8 (NativeKeyValueArrays_2_t31F3CE04B96C7A9192BEB52001190AF4216A7990* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t31F3CE04B96C7A9192BEB52001190AF4216A7990*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m5D960A4C192B677E942F8EE6033822A69C5F22F8_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m08640C8B3F96CF59CE7C409ED1E5A53510CE219F (NativeKeyValueArrays_2_t699CBC5BC6A801E2B73A1521AF599E34499857B8* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t699CBC5BC6A801E2B73A1521AF599E34499857B8*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m08640C8B3F96CF59CE7C409ED1E5A53510CE219F_gshared)(__this, method);
}
inline BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 UnsafeUtility_ReadArrayElement_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m80C61348EC8F9D4F73F0AAA4A69A028E6D599DCD_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m80C61348EC8F9D4F73F0AAA4A69A028E6D599DCD_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m0DD220DB30C031B6200A4119D0DCF56B565846E6 (NativeKeyValueArrays_2_tD4417ED5E6C2DAC8437C9159D79F622F83C72D39* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tD4417ED5E6C2DAC8437C9159D79F622F83C72D39*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m0DD220DB30C031B6200A4119D0DCF56B565846E6_gshared)(__this, method);
}
inline GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 UnsafeUtility_ReadArrayElement_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m2A66DBE83F9498AC60B42C9A0B1C7E4401B4CDCC_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m2A66DBE83F9498AC60B42C9A0B1C7E4401B4CDCC_gshared_inline)(___0_source, ___1_index, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m8C99316414D47C54EF1AFF05D069266C1FE65840 (NativeKeyValueArrays_2_t4AA59D24D1616F48DCDB5CB6C4ABA336C51B1764* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_t4AA59D24D1616F48DCDB5CB6C4ABA336C51B1764*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m8C99316414D47C54EF1AFF05D069266C1FE65840_gshared)(__this, method);
}
inline int32_t NativeKeyValueArrays_2_get_Length_m4A745207D9267220A0F145A1E29332EF7D212017 (NativeKeyValueArrays_2_tA9D90BDB6A6A5B59E496BEBBDA8082D2A0598A49* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (NativeKeyValueArrays_2_tA9D90BDB6A6A5B59E496BEBBDA8082D2A0598A49*, const RuntimeMethod*))NativeKeyValueArrays_2_get_Length_m4A745207D9267220A0F145A1E29332EF7D212017_gshared)(__this, method);
}
inline bool UnsafeUtility_ReadArrayElement_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mF765CB2E5FD631DFB79C25DE656F3C1BB359B995_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	return ((  bool (*) (void*, int32_t, const RuntimeMethod*))UnsafeUtility_ReadArrayElement_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mF765CB2E5FD631DFB79C25DE656F3C1BB359B995_gshared_inline)(___0_source, ___1_index, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t ConnectionId_GetHashCode_m7CD005F8169BE20415267EA691CCBEAC252DE0F2 (ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB* __this, const RuntimeMethod* method) ;
inline void Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271 (uint8_t* ___0_pointer, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___1_allocator, const RuntimeMethod* method)
{
	((  void (*) (uint8_t*, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148, const RuntimeMethod*))Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_gshared)(___0_pointer, ___1_allocator, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t DrawKey_GetHashCode_m6E3C5F3D42D02D8035AA7E7B5501FDBC1551F4E5 (DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline (EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t FixedString128Bytes_GetHashCode_mB211F7E224953364EE91770921BA59760A0E4428 (FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Int32_GetHashCode_m253D60FF7527A483E91004B7A2366F13E225E295 (int32_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Int64_GetHashCode_mDB050BE2AC244D92B14D1DF725AAD279CDC48496 (int64_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t NetworkEndpoint_GetHashCode_mA1753E7F3CD5AC227330A2A63F6623BA737A7696 (NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t RangeKey_GetHashCode_mC38845B6A1CC657D6F6B1149E7448AA6A3EF3257 (RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SharedInstanceHandle_GetHashCode_m5B97E179A78BD59969291F66E286E00873FC120D (SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UInt16_GetHashCode_m534E5103D0DA9C6FCED4F2F007993D3E38165200 (uint16_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UInt32_GetHashCode_mB9A03A037C079ADF0E61516BECA1AB05F92266BC (uint32_t* __this, const RuntimeMethod* method) ;
inline void UnsafeUtility_InternalCopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mFB7A3FF45C4521DC402828AC45C2220BC22ACF0A (uint8_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method)
{
	((  void (*) (uint8_t*, void*, const RuntimeMethod*))UnsafeUtility_InternalCopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mFB7A3FF45C4521DC402828AC45C2220BC22ACF0A_gshared)(___0_input, ___1_ptr, method);
}
inline void UnsafeUtility_InternalCopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m305D0FDE533B9169C6E61EDFEC9CFB0564E7B14B (int32_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method)
{
	((  void (*) (int32_t*, void*, const RuntimeMethod*))UnsafeUtility_InternalCopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m305D0FDE533B9169C6E61EDFEC9CFB0564E7B14B_gshared)(___0_input, ___1_ptr, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPlane_tB7D8CC6F7AACF5F3AA483AF005C1102A8577BC0C_m0E2598C9ADF3B0AEE17BF48ED32D575C5CEA235F_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Plane_tB7D8CC6F7AACF5F3AA483AF005C1102A8577BC0C);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m5C149DAFA4891018B2A908B539ECEA7344A76895_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_m01BA62EFADDA507E23FF44FE03830FD3B5F78FA3_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5_m438B3608DBFD87F5F67078E3057DC5924AC8195C_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRaycastHit2D_t3EAAA06E6603C6BC61AC1291DD881C5C1E23BDFA_m9F2E6900414B8E79D73B79153C0C09E4B096E026_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RaycastHit2D_t3EAAA06E6603C6BC61AC1291DD881C5C1E23BDFA);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisReadCommand_t5DB46BD58D686FDDFBD8AB7600B9CF676DC7D97F_m77A4844C56CF14E60F635BF8B9328F24EB3D5700_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ReadCommand_t5DB46BD58D686FDDFBD8AB7600B9CF676DC7D97F);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m0DD27F56FCBCF16FDFFBD1EE27D800D1E12E1159_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRenderStateBlock_tFC570EF2C8F3A817FECD578E385D18CEEEA06733_mC76B85D6E5D028D1CBA6D13DA4D8D6F22BC49A40_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RenderStateBlock_tFC570EF2C8F3A817FECD578E385D18CEEEA06733);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRenderTargetIdentifier_tA528663AC6EB3911D8E91AA40F7070FA5455442B_m4E7BCF1E5C9A58F177D597D11C2F9F22DD6EC981_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RenderTargetIdentifier_tA528663AC6EB3911D8E91AA40F7070FA5455442B);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRendererListLegacyResource_tEF05A444F7845E04F5E6568549AF26D434AD1B68_m06EC00A36B22580D47FAFB59C10F11D4926D1C73_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RendererListLegacyResource_tEF05A444F7845E04F5E6568549AF26D434AD1B68);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRendererListResource_tCB75EF1874F8A294101A45F937987CC314B92214_m28F3FFC02455138821B6DF03E280A8EAD4C2F738_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RendererListResource_tCB75EF1874F8A294101A45F937987CC314B92214);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisResourceHandle_tD3B1FFBD59EB9C23F0A020351836F834C4BD276C_m7623BE82F9B85945CC45CDE4CF5A13852FAD9E4B_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ResourceHandle_tD3B1FFBD59EB9C23F0A020351836F834C4BD276C);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisResourceReaderData_t1B57A9C4CC76875899745E115AA53FF40C6412EC_m3D52C3A1605A0E3F0F527B385F76164833DDE196_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ResourceReaderData_t1B57A9C4CC76875899745E115AA53FF40C6412EC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisResourceUnversionedData_t3F4B539E7806E513C53A94EAABF5F969AAA384CC_m10B3654FED9F7957136F171462DD8585D507B91E_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ResourceUnversionedData_t3F4B539E7806E513C53A94EAABF5F969AAA384CC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisResourceVersionedData_tC935A106FCF6C0800974D2C98DBE14E19A19B1DC_mB32D92F787103CE84A8E237127303C3409C96868_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ResourceVersionedData_tC935A106FCF6C0800974D2C98DBE14E19A19B1DC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0_mE6E787848CEE23EB1C0D0080C9066ED3BF24E2F0_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ShaderTagId_t453E2085B5EE9448FF75E550CAB111EFF690ECB0);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisShadowSliceData_t1BCFEDC63BECA994949FE1F4245CEE930EE69E20_m956739B4FBBB8047A0055E42D9C6ED3A8469D3D1_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ShadowSliceData_t1BCFEDC63BECA994949FE1F4245CEE930EE69E20);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisShadowSplitData_tC276A96F461DD73CFF6D94DB557D42A1643640DF_m3D37A8E09F54E22A8C98CFE67CCB1540F351FF0E_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ShadowSplitData_tC276A96F461DD73CFF6D94DB557D42A1643640DF);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_m5B31FC0D8A4C03E761E9BEBB8D70C6A52621D290_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m2997ADC74783B1D48AA40D06283B3D03ADDCDCE7_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(float);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSmallEntityIdArray_t60189E3B3EEF42ED87B04DBA8511AAED039018EE_m1B029F3F0690173002F9B234DCC60E038953FFCC_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SmallEntityIdArray_t60189E3B3EEF42ED87B04DBA8511AAED039018EE);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSphericalHarmonicsL2_tCBFB646455D2069E738976E5B745C6DF023B6BA2_mC70959E495A4D1C94B4213120D458C6D0BCBDC43_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SphericalHarmonicsL2_tCBFB646455D2069E738976E5B745C6DF023B6BA2);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisStoreAudit_t9E8FF144788FDFF9C68E912C5BB87C533F860292_m1E3AB1A3BB8C21B5D25F836F4B1A207A1C79E3D0_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(StoreAudit_t9E8FF144788FDFF9C68E912C5BB87C533F860292);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSubMeshDescriptor_t699E32E3F27A97CF89B0030F74C82D5FB7DEF934_m0E6123BD801FC61D10E0AF6C3AD0C11C55E2FAC3_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SubMeshDescriptor_t699E32E3F27A97CF89B0030F74C82D5FB7DEF934);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSubPassDescriptor_t912FE0FF4C99BF293A1E4442353C35B2BB8997A9_m31787ED82385A6C4DBABA5D797A6AD5F16E84FD2_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SubPassDescriptor_t912FE0FF4C99BF293A1E4442353C35B2BB8997A9);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSubviewOcclusionTest_t4C10094E5EF2C745723FEFE4E5749FBB75CAA026_m10410672D418D98452073937AA7CF8F910F3ED12_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SubviewOcclusionTest_t4C10094E5EF2C745723FEFE4E5749FBB75CAA026);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTileAnimationEntityIdData_t167D5D62289EB4EDFE19B374BF7E079DFE94677D_mCF24178CA655D3FEFC1211A2C65BDEE3657558B2_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TileAnimationEntityIdData_t167D5D62289EB4EDFE19B374BF7E079DFE94677D);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTileData_tFB814629D010ABD175127C0BE96FD96EA606E00F_m9B7CD177A7FCFF5EFF632A9DB926A794665597F3_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TileData_tFB814629D010ABD175127C0BE96FD96EA606E00F);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTransformUpdatePacket_t056014168D7AE17359B1BD85E70A6E1B43C3AB18_m2E3A8AAA861DC7322E3202D48CF9411EDEC667CA_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TransformUpdatePacket_t056014168D7AE17359B1BD85E70A6E1B43C3AB18);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTreeInstance_t382B018173ED020660D262061EA9424682614F50_mA9A8BE70192E054FB915B010AABA2E656B3A44C1_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TreeInstance_t382B018173ED020660D262061EA9424682614F50);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mA6131DF2439BB55C289E1DFA433D0DD92D7BA5E2_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(uint16_t);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m809F14D45C4DEBB28BC7EE404B1BAE0D266DA45D_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(uint32_t);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_mB9F4978F2FA61BAAE1C0E98A406752B8A91F2A74_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(uint64_t);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisURPLightShadowCullingInfos_t8EBC5966B6C0C703C739850EA3B585324022F0E9_m0F106770D19EA69884FC443A86FF1DC3DB36F158_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(URPLightShadowCullingInfos_t8EBC5966B6C0C703C739850EA3B585324022F0E9);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7_m14B147EA44DA5DF81D6A8196B7DB2F46248BA676_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A_mDDE438C614E894787D93830E64073E1672A60F62_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Vector2Int_t69B2886EBAB732D9B880565E18E7568F3DE0CE6A);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_mA49B1CD89B97B9BCB272FE386C3BE01FB8B72291_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVector3Int_t65CB06F557251D18A37BD71F3655BA836A357376_m03654384927B0DBE78F89D34C39C49285997E47D_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Vector3Int_t65CB06F557251D18A37BD71F3655BA836A357376);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3_mDF2841BA1AA00B8E3F1448B115C56DA57B38BEDC_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVertex_t016AC68A2E6C62576E65412BEC71544AFC01AFC7_m098A8B4D7A1FC28B7F05C1145DFCCDD73DA2E356_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Vertex_t016AC68A2E6C62576E65412BEC71544AFC01AFC7);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVertexAttributeDescriptor_tD4231FBF57335465D16308D2A18E8E83D36BFA76_m618D0E6DCE238EE7C84033E17EBF7CD433DE4EC3_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(VertexAttributeDescriptor_tD4231FBF57335465D16308D2A18E8E83D36BFA76);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVisibleLight_t0A4DF5B22865A00F618A0352B805277FA0132805_m862C182B922F37058B443466E468D1248237E021_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(VisibleLight_t0A4DF5B22865A00F618A0352B805277FA0132805);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisVisibleReflectionProbe_t8AF1FAD09A36E33F5101B683DB8E99582815EF0B_mC6B333C878918DC375A5CA3B6D164910AD8265AF_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(VisibleReflectionProbe_t8AF1FAD09A36E33F5101B683DB8E99582815EF0B);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisIl2CppFullySharedGenericAny_mD5748385042811E049C06FFD865AFD213C5FA9EB_gshared (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t77C4AF644C0D2AE4F0E9399E7AED3FED3426FBD7 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	{
		uint32_t L_0 = SizeOf_T_t77C4AF644C0D2AE4F0E9399E7AED3FED3426FBD7;
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_Tisfloat2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA_mC49524796957DAD302EFB36E09DD448926DB456A_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(float2_t24AA5C0F612B0672315EDAFEC9D9E7F1C4A5B0BA);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_Tisfloat3_t4AB5D88249ADB24F69FFD0793E8ED25E1CC3745E_m70D4F33F8703BECEA4908C6A9A79097EDC114F66_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(float3_t4AB5D88249ADB24F69FFD0793E8ED25E1CC3745E);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_Tisfloat4_t89D9A294E7A79BD81BFBDD18654508532958555E_mBE3C4435AC121DAB1DAF32FA1608A34F559B1DAE_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(float4_t89D9A294E7A79BD81BFBDD18654508532958555E);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_Tisfloat4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2_m624EE72C32F295D2E0A8012AD62DDD61343E4D86_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(float4x4_t7EDD16F7F57DC7F61A6302535F7C19FB97915DF2);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m07A2175124C13DFFE3988379EBF825963D3B0CEC_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_Tisquaternion_tD6BCBECAF088B9EBAE2345EC8534C7A1A4C910D4_m72C06AEE10049E6AFECD925E4E5E8FFD20DB35A6_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(quaternion_tD6BCBECAF088B9EBAE2345EC8534C7A1A4C910D4);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRTInstance_t82A289FDC8E8112219057734D0AE172A4E822AED_m7D99537B1CA0D394297776F01407D359ADBD8245_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(RTInstance_t82A289FDC8E8112219057734D0AE172A4E822AED);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisShadowResolutionRequest_tC1B869ADCA139D8D7AD8A355373F84C7F5E0FCBE_m2E2295654667EF824CC228AB89C2CDE03F4B5F5A_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ShadowResolutionRequest_tC1B869ADCA139D8D7AD8A355373F84C7F5E0FCBE);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSample_t3BE5915021631BD79173E66EC224867714256407_m0499D7DBE91FC662A4CBEAA7FA473784BC601E24_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Sample_t3BE5915021631BD79173E66EC224867714256407);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisBaselib_RegisteredNetwork_CompletionResult_t4C07F67F2F155FE19DBE7A41393038A154301CC7_mFA61A3122B70E80C7F9AB4BA0825869530A9F863_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Baselib_RegisteredNetwork_CompletionResult_t4C07F67F2F155FE19DBE7A41393038A154301CC7);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisBaselib_RegisteredNetwork_Request_t6AF2E70EE04D8C3608FB27091CDE9B4A25A9A4FE_m9FA4432682C1AB20FCD856337EE59A0158D0020D_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Baselib_RegisteredNetwork_Request_t6AF2E70EE04D8C3608FB27091CDE9B4A25A9A4FE);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisBlock_tBD2149E220F819A66A4A0A18F237932ACD9011DE_m743599B49DFDAE3F0F188376316D0ADFD09F4F51_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Block_tBD2149E220F819A66A4A0A18F237932ACD9011DE);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m220E6E2F21EDDA205CB7B873605CF4F435AC133D_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisHostnameLookupData_t1B68029C2268ABA3B91249AEEA15B0E4387ACB1F_m78AEBAB24D86C57A1D2D8D3C45F3E7504F986064_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(HostnameLookupData_t1B68029C2268ABA3B91249AEEA15B0E4387ACB1F);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisIncomingDisconnection_tD1F2DDEC6E7112BF5659DFF4FFD8DE263CF7F294_mCE453ACF8BC6EA9A43EACCA17A58E9477CFCCD67_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(IncomingDisconnection_tD1F2DDEC6E7112BF5659DFF4FFD8DE263CF7F294);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPlanePacket4_t2954005DBF78AC180CF45B652536CC2F5158D54B_mDE4FDCE59FA3B4E4A984BFEF462ECD6AE13B4D4E_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PlanePacket4_t2954005DBF78AC180CF45B652536CC2F5158D54B);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSplitInfo_t708E0734C9BC407BA5882105A9721756605C913A_m4BC2E3F456265B3148DB8A9B0B96822A81627B29_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SplitInfo_t708E0734C9BC407BA5882105A9721756605C913A);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisGeoPoolEntrySlot_tE1B7B103A90E4C3379CED82C8D6DA55189422977_m5FB64B99E3A734CF79CB83CCAC7F0CF4F5A0D858_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(GeoPoolEntrySlot_tE1B7B103A90E4C3379CED82C8D6DA55189422977);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisMeshChunk_tEEA891FD6A9FBF660DB8ED98B23FCCB8A413C2BD_m37461B1DCD9D8B5939941C8CDBA060C869F6C26C_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(MeshChunk_tEEA891FD6A9FBF660DB8ED98B23FCCB8A413C2BD);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisIPCData_t46E9604572E2A38CBBA8A418C6E25E6BAE2757CC_m3454D826D8082501A7BA8432DED11D3EBCCC40FF_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(IPCData_t46E9604572E2A38CBBA8A418C6E25E6BAE2757CC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m1BC41FFFDC1F1529A8FFCDDE79D9F90672AE27A9_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisInfo_tA3039772991DEEDBC29A00439A055C5166133A27_m6AF959503971FA21E5276901A79319B62D998349_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Info_tA3039772991DEEDBC29A00439A055C5166133A27);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisInfo_tDBEB127ABB26184014A541C0CAD1FC8D1B95DE84_mFD6D4DC06FDBCF90BE9369EAAE7B382D6E9281B6_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Info_tDBEB127ABB26184014A541C0CAD1FC8D1B95DE84);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRequest_tAA55F47806E39B0E19B53273DCBFB5CF457F9187_mD235BFB13BCC58EBBECCBC6481287858BE8BCF43_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Request_tAA55F47806E39B0E19B53273DCBFB5CF457F9187);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisHitEntry_tB5C47A91F398525506E16A47748007BC0EF8FD4C_m64D9F97E55FFEC3B5D220FE23BDBAE67C5F0E49B_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(HitEntry_tB5C47A91F398525506E16A47748007BC0EF8FD4C);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisGpuMaterialEntry_t4A4728FB9206E4E82C457E3339E0E00FD904518F_mCA13D2FD66EC4B8C8DA1ECC03256FF1F371582A2_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(GpuMaterialEntry_t4A4728FB9206E4E82C457E3339E0E00FD904518F);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisBackgroundRepeatInstance_t2D7A8E1F6278188BE2026DF769C49A975D38B12D_mDC360FD5B4CA12C3E1CB9389792BF871755750CF_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(BackgroundRepeatInstance_t2D7A8E1F6278188BE2026DF769C49A975D38B12D);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTessellationJobParameters_tA2407D5C15761590BDD217DDE4861964544E8CE2_m8BD71ABFC754B037C3E32A5E8A33D0316D741197_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TessellationJobParameters_tA2407D5C15761590BDD217DDE4861964544E8CE2);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSubQueueItem_tAE6DA2A0B68107490190DB4680442767B9D420B7_m161BA5C43ACD517979DC256CE143684CF877C406_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SubQueueItem_tAE6DA2A0B68107490190DB4680442767B9D420B7);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPipelineImpl_t422927EB35F759F18ECBA1C7B075AAC92918017E_m2C26F7F17531BBF855179642C809792A42D0E48A_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PipelineImpl_t422927EB35F759F18ECBA1C7B075AAC92918017E);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_m0388FD293710FE7037546DF5F05B5B35350E7416_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mF549CA6CE5BFE9827DFA58A3C694D474A60E1D3A_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisOccluderContextSlot_t963DBFFF1612E4108D0BEB42369F78758BE71D5D_mABB697B77F4D8914DA64616371600595F95A4649_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(OccluderContextSlot_t963DBFFF1612E4108D0BEB42369F78758BE71D5D);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPainter2DJobData_t227572FEAE4A071ED0378501E752A72FF0ACC4EF_m5E28FB6FC41409886830E4452162ABD614D6B1EE_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Painter2DJobData_t227572FEAE4A071ED0378501E752A72FF0ACC4EF);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTransformWriteTween_tA1E06E591FBB1696DE2915757CCA39C6B6E074D0_mD72D2AC6757AC52BEF48CB2D31F5F0F69225F817_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TransformWriteTween_tA1E06E591FBB1696DE2915757CCA39C6B6E074D0);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisBrick_tE6E9230DFDF650A631C116E79FB28F41618C3CE0_mFF376C07B4C08F05805D3B2BBA3C6F63A3ED1A55_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Brick_tE6E9230DFDF650A631C116E79FB28F41618C3CE0);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisInternalQosServer_t56825CF99AB62B0FD12D69A5E5EB72FA463E687A_m0A530D3313C1BA2E4815A01B8C3954EB2FF51576_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(InternalQosServer_t56825CF99AB62B0FD12D69A5E5EB72FA463E687A);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSplitInfo_tBD1436BC99CBBC9658FA9219EB22657F757C4A1A_m1091DCDAAE80F40BF9FEBC5CB9B336BF292D3410_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(SplitInfo_tBD1436BC99CBBC9658FA9219EB22657F757C4A1A);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisCompiledPassInfo_t0602472C646D539122A25EDD89C2E65F136A1948_m77AFA028654E323CD45C969587ED52357C15DC5D_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(CompiledPassInfo_t0602472C646D539122A25EDD89C2E65F136A1948);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisCompiledResourceInfo_t22204344249241C372CFC608709931F7EEEF733C_mB301D622E2F408954E25D4C03B61832EAC07CC0D_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(CompiledResourceInfo_t22204344249241C372CFC608709931F7EEEF733C);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisLightData_tAC4023737E9903DE3F96B993AA323E062ABCB9ED_m715B4D1DE5168BB55FE2996CF6818659D00DD52A_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(LightData_tAC4023737E9903DE3F96B993AA323E062ABCB9ED);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisSlot_tA2F29CF08EAE46C3E2B6D96DCD7C96BF887A6127_mC0B012CE2E393FEFB9CF495257ADBED6E0AC0B15_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Slot_tA2F29CF08EAE46C3E2B6D96DCD7C96BF887A6127);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPendingPacket_t479815078D1E4D1B5441E44C39E685232AB4EE94_m9B045671FF9C0AB2DE5C20B19C9E5E33E023F60A_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PendingPacket_t479815078D1E4D1B5441E44C39E685232AB4EE94);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPendingSend_tC3CE84554C44A80712A3DA2E69BD887F0300440E_m9B3B83954941B41D743D69A89D7B61A3637C97E7_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PendingSend_tC3CE84554C44A80712A3DA2E69BD887F0300440E);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisOutputVertex_tA4BCEA851534A5A22BE19AD8D0F20102B11F88C4_m62FAA1E1FA633DC7B9190D70326383266CE88DB5_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(OutputVertex_tA4BCEA851534A5A22BE19AD8D0F20102B11F88C4);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisRune_tE8DB196113D1DBF48E710C120CC50D2AD7D5915E_m459B4E23FA6FDEAF665A2D9C71BFFD04E324AAE9_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(Rune_tE8DB196113D1DBF48E710C120CC50D2AD7D5915E);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisLightDescriptor_tCFFA8945B9D03CA49332A3243AAD453F0EFFD8C5_m280BB54DCC778A0D3881798404261DB493A6754F_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(LightDescriptor_tCFFA8945B9D03CA49332A3243AAD453F0EFFD8C5);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisBodyUpdateTarget_t6DFEB7E2EB41937A4D139AAA98A0B2FEF61145DC_m334F5512415F5F94EF578D83ADB66B59954610FE_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(BodyUpdateTarget_t6DFEB7E2EB41937A4D139AAA98A0B2FEF61145DC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisContactBeginTarget_t219EBBED83B122E8CC4DEE88C04DF6F8595882CE_m58E6AA95BA218D79ED21AACA058818C6E99B98AB_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ContactBeginTarget_t219EBBED83B122E8CC4DEE88C04DF6F8595882CE);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisContactEndTarget_t6AFFF294A99330ACB933D8C200E7CA75AEF31B29_mCA678B18B8781C28B2A150B5A8A2D943C3226C04_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(ContactEndTarget_t6AFFF294A99330ACB933D8C200E7CA75AEF31B29);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisJointThresholdTarget_tD35F9E05A70D01669DD20FE508E449AF631338FC_mAA957CCA88A5A06E5E9E75793E87FCA9AE830DD5_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(JointThresholdTarget_tD35F9E05A70D01669DD20FE508E449AF631338FC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTriggerBeginTarget_t2E0A2F6FBE5E226EE40F7AC2348034FDA9BAB5E7_m928E508FE4548E004BD25773EDD5FA7E63DE661B_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TriggerBeginTarget_t2E0A2F6FBE5E226EE40F7AC2348034FDA9BAB5E7);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisTriggerEndTarget_tE04F3C90BD5BD9EADC98FFFDDA469061A72A3CD9_m5C517D8CB28BCDA8E45BBCCA28F6D29E49345670_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(TriggerEndTarget_tE04F3C90BD5BD9EADC98FFFDDA469061A72A3CD9);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisCapsuleGeometryElement_t7283C3F74372B1D8035F99314DCDAFBB9137D164_m03B4FFCD9133E5968F660ACB1C69F201550869B4_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(CapsuleGeometryElement_t7283C3F74372B1D8035F99314DCDAFBB9137D164);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisCircleGeometryElement_t49610DEDF104EC781DEA6886ECE01A85F007D4AC_m0AEA8CD0AEB9F3B053D6DD36B45F3A73CA388A8B_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(CircleGeometryElement_t49610DEDF104EC781DEA6886ECE01A85F007D4AC);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisLineElement_t96DCC25AC17109ACE2D8E83D154E5B7559953339_mC2E2E62C86F69A87A799A96ADEE12B1ADE4EB6D6_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(LineElement_t96DCC25AC17109ACE2D8E83D154E5B7559953339);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPointElement_tFE435D22E766645936C545EC265D1E41C047D9DE_mBC15C25DBFE1E824F5AFD866A480763E5EAEBAAB_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PointElement_tFE435D22E766645936C545EC265D1E41C047D9DE);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisPolygonGeometryElement_t6C8FF21F16A40EA03BE69163226AF7366F22DCFD_m6AD36825664C7DA35314D7D484A334C6EFADCCDB_gshared (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(PolygonGeometryElement_t6C8FF21F16A40EA03BE69163226AF7366F22DCFD);
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_WriteUnaligned_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m61F5534E1C33FE40E5D929CE8D9F6D55C0022CC9_gshared (uint8_t* ___0_destination, uint32_t ___1_value, const RuntimeMethod* method) 
{
	{
		uint8_t* L_0 = ___0_destination;
		uint32_t L_1 = ___1_value;
		*(uint32_t*)L_0 = L_1;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_WriteUnaligned_TisIl2CppFullySharedGenericAny_mF9DAA41D685A65A581864DBF65CE23C6F7E72BDF_gshared (uint8_t* ___0_destination, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_tBBA6F3D6E1BC76807F91C471253B4033DB66D6E8 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_tBBA6F3D6E1BC76807F91C471253B4033DB66D6E8);
	{
		uint8_t* L_0 = ___0_destination;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_value : &___1_value), SizeOf_T_tBBA6F3D6E1BC76807F91C471253B4033DB66D6E8);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_0, L_1, SizeOf_T_tBBA6F3D6E1BC76807F91C471253B4033DB66D6E8);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->rgctx_data, 0), (void**)(Il2CppFullySharedGenericAny*)L_0, (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_Add_TisIl2CppFullySharedGenericAny_mF9DBFAB669D56EA513D40590689EAFBAA90CCED6_gshared (Il2CppFullySharedGenericAny* ___0_source, int32_t ___1_elementOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_tEEBA3842B319E4DD94173281BA41EA604298A6D8 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		int32_t L_1 = ___1_elementOffset;
		uint32_t L_2 = SizeOf_T_tEEBA3842B319E4DD94173281BA41EA604298A6D8;
		return ((Il2CppFullySharedGenericAny*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)il2cpp_codegen_multiply(L_1, ((intptr_t)L_2)))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_Add_TisIl2CppFullySharedGenericAny_mFE62A3CA45207936620FBCA929F276C3236012A8_gshared (Il2CppFullySharedGenericAny* ___0_source, intptr_t ___1_elementOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t0DA05DAAE1602D536B9336ECFD5023C7250185BB = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		intptr_t L_1 = ___1_elementOffset;
		uint32_t L_2 = SizeOf_T_t0DA05DAAE1602D536B9336ECFD5023C7250185BB;
		return ((Il2CppFullySharedGenericAny*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)il2cpp_codegen_multiply(L_1, (int32_t)L_2))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void* Unsafe_Add_TisIl2CppFullySharedGenericAny_mB1EC0E0AE7ED01ECA395F784DC4E6399B4A49C60_gshared (void* ___0_source, int32_t ___1_elementOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t3DF4F371C547F52D537F3A819B23EB9BEC9C55AE = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_elementOffset;
		uint32_t L_2 = SizeOf_T_t3DF4F371C547F52D537F3A819B23EB9BEC9C55AE;
		return ((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)il2cpp_codegen_multiply(L_1, ((intptr_t)L_2)))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_AddByteOffset_TisIl2CppFullySharedGenericAny_mB359436BF000AA65EE37E915C2DC1E46124CD5F9_gshared (Il2CppFullySharedGenericAny* ___0_source, intptr_t ___1_byteOffset, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		intptr_t L_1 = ___1_byteOffset;
		return ((Il2CppFullySharedGenericAny*)il2cpp_codegen_add((intptr_t)L_0, L_1));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Unsafe_AreSame_TisIl2CppFullySharedGenericAny_m973E84A89F7F53EC2D778BACC2721876A86D969A_gshared (Il2CppFullySharedGenericAny* ___0_left, Il2CppFullySharedGenericAny* ___1_right, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_left;
		Il2CppFullySharedGenericAny* L_1 = ___1_right;
		return (bool)((((RuntimeObject*)(intptr_t)L_0) == ((RuntimeObject*)(intptr_t)L_1))? 1 : 0);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Unsafe_As_TisRuntimeObject_mFFE457F6F9B8F4E52063D06898B25FF13D88510C_gshared (RuntimeObject* ___0_o, const RuntimeMethod* method) 
{
	{
		RuntimeObject* L_0 = ___0_o;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_As_TisIl2CppFullySharedGenericAny_TisIl2CppFullySharedGenericAny_mD037EB4B68BC5EA079423C76C72D223978641B78_gshared (Il2CppFullySharedGenericAny* ___0_source, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		return (Il2CppFullySharedGenericAny*)(L_0);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void* Unsafe_AsPointer_TisIl2CppFullySharedGenericAny_mF2E1E4110F5A7C17AA517EDBC6C2EBC79240552E_gshared (Il2CppFullySharedGenericAny* ___0_value, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_value;
		return (void*)(((uintptr_t)L_0));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Array32768_1_tF94DB9E949B98E267CCEE7E61378AA0A89C951D6* Unsafe_AsRef_TisArray32768_1_tF94DB9E949B98E267CCEE7E61378AA0A89C951D6_mA61D216FC909FB8B42CB2EDBD3146EF82808E8D9_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (Array32768_1_tF94DB9E949B98E267CCEE7E61378AA0A89C951D6*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t* Unsafe_AsRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE30782588C12082D21B776E14FAB37229016AD0E_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (int32_t*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t* Unsafe_AsRef_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m58D36DD43D19976220C297D3D667BCAA293D40D0_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (int64_t*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t* Unsafe_AsRef_TisIntPtr_t_mE17A0ECBFCF763A4C065A542CDF0F425DEFC0CDA_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (intptr_t*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* Unsafe_AsRef_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_m55889A5425117884EC38A5171F72C0C46A4FEBD9_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (Long1024_tEE887C506947419DC829213E6C7483D80AF5659F*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StreamCompressionModel_t7F9FB7DB2D88DA832A4DD61C1E6587A4C530697D* Unsafe_AsRef_TisStreamCompressionModel_t7F9FB7DB2D88DA832A4DD61C1E6587A4C530697D_m4FAA5C77EACDEEC07D84980F8AD42ACCAE596203_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (StreamCompressionModel_t7F9FB7DB2D88DA832A4DD61C1E6587A4C530697D*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_AsRef_TisIl2CppFullySharedGenericAny_mAAD2E0C13B109573A6679E602EB320A6FEFFAFEE_gshared (Il2CppFullySharedGenericAny* ___0_source, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_AsRef_TisIl2CppFullySharedGenericAny_m32B7913D5C99D58667B8F989FEFA15BF74484607_gshared (void* ___0_source, const RuntimeMethod* method) 
{
	int32_t* V_0 = NULL;
	{
		void* L_0 = ___0_source;
		V_0 = (int32_t*)L_0;
		int32_t* L_1 = V_0;
		return (Il2CppFullySharedGenericAny*)(L_1);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t Unsafe_ByteOffset_TisIl2CppFullySharedGenericAny_mA6E826435E2E45CEB6CA4EEE2272084E98C9ECBA_gshared (Il2CppFullySharedGenericAny* ___0_origin, Il2CppFullySharedGenericAny* ___1_target, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___1_target;
		Il2CppFullySharedGenericAny* L_1 = ___0_origin;
		return (intptr_t)((Il2CppFullySharedGenericAny*)il2cpp_codegen_subtract((intptr_t)L_0, (intptr_t)L_1));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_Copy_TisIl2CppFullySharedGenericAny_m90F3C1C2EF81A6895829434FD9F01F9CEBC2C007_gshared (Il2CppFullySharedGenericAny* ___0_destination, void* ___1_source, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_tC3719F9FE9415CD0DD37B7BEB6C7775EB0F8DE9C = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_T_tC3719F9FE9415CD0DD37B7BEB6C7775EB0F8DE9C);
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_destination;
		void* L_1 = ___1_source;
		il2cpp_codegen_memcpy(L_2, L_1, SizeOf_T_tC3719F9FE9415CD0DD37B7BEB6C7775EB0F8DE9C);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_0, L_2, SizeOf_T_tC3719F9FE9415CD0DD37B7BEB6C7775EB0F8DE9C);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->rgctx_data, 1), (void**)(Il2CppFullySharedGenericAny*)L_0, (void*)L_2);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_Copy_TisIl2CppFullySharedGenericAny_mEE2EE2F7EB4F5E257ED23286D135C0B5FBD2938A_gshared (void* ___0_destination, Il2CppFullySharedGenericAny* ___1_source, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t9CF1D887FD7AAE86B8801FE9FCDA9658F0727699 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_T_t9CF1D887FD7AAE86B8801FE9FCDA9658F0727699);
	{
		void* L_0 = ___0_destination;
		Il2CppFullySharedGenericAny* L_1 = ___1_source;
		il2cpp_codegen_memcpy(L_2, L_1, SizeOf_T_t9CF1D887FD7AAE86B8801FE9FCDA9658F0727699);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_0, L_2, SizeOf_T_t9CF1D887FD7AAE86B8801FE9FCDA9658F0727699);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->rgctx_data, 1), (void**)(Il2CppFullySharedGenericAny*)L_0, (void*)L_2);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Unsafe_IsAddressGreaterThan_TisIl2CppFullySharedGenericAny_m42FCC0BC393819A6484693DF4DB5133ECCE66C41_gshared (Il2CppFullySharedGenericAny* ___0_left, Il2CppFullySharedGenericAny* ___1_right, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_left;
		Il2CppFullySharedGenericAny* L_1 = ___1_right;
		return (bool)((!(((RuntimeObject*)(uintptr_t)L_0) <= ((RuntimeObject*)(uintptr_t)L_1)))? 1 : 0);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Unsafe_IsAddressLessThan_TisIl2CppFullySharedGenericAny_mCC9684D6F21A286D4EDC665E80CC283858FC64BA_gshared (Il2CppFullySharedGenericAny* ___0_left, Il2CppFullySharedGenericAny* ___1_right, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_left;
		Il2CppFullySharedGenericAny* L_1 = ___1_right;
		return (bool)((!(((RuntimeObject*)(uintptr_t)L_0) >= ((RuntimeObject*)(uintptr_t)L_1)))? 1 : 0);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_Read_TisIl2CppFullySharedGenericAny_m0E58D1996E4A4EC30A7E7480B233649D88C742D4_gshared (void* ___0_source, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t69374FF7455BF0198FC0F2F50A723741E052FE21 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_t69374FF7455BF0198FC0F2F50A723741E052FE21);
	{
		void* L_0 = ___0_source;
		il2cpp_codegen_memcpy(L_1, L_0, SizeOf_T_t69374FF7455BF0198FC0F2F50A723741E052FE21);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_T_t69374FF7455BF0198FC0F2F50A723741E052FE21);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_ReadUnaligned_TisIl2CppFullySharedGenericAny_m8BB18303C08EE3FAF754D4BDAC68EBF56F2F03B2_gshared (uint8_t* ___0_source, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t4C0A03328C9C8A254123851D32EB82133A725365 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_t4C0A03328C9C8A254123851D32EB82133A725365);
	{
		uint8_t* L_0 = ___0_source;
		il2cpp_codegen_memcpy(L_1, L_0, SizeOf_T_t4C0A03328C9C8A254123851D32EB82133A725365);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_T_t4C0A03328C9C8A254123851D32EB82133A725365);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_ReadUnaligned_TisIl2CppFullySharedGenericAny_m6ECE237E350B3B76FAABD2433F53D4A3E310FE39_gshared (void* ___0_source, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t46236521BFD11B8A5B1BABCEAD715D49ED4A1A28 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_t46236521BFD11B8A5B1BABCEAD715D49ED4A1A28);
	{
		void* L_0 = ___0_source;
		il2cpp_codegen_memcpy(L_1, L_0, SizeOf_T_t46236521BFD11B8A5B1BABCEAD715D49ED4A1A28);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_T_t46236521BFD11B8A5B1BABCEAD715D49ED4A1A28);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Unsafe_SizeOf_TisIl2CppFullySharedGenericAny_mB5ECF717DD678B54E713D441A3B4DFB15CB3FEC1_gshared (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t1FDEFCA0B0E530419C707125EE8E74E3221D3778 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	{
		uint32_t L_0 = SizeOf_T_t1FDEFCA0B0E530419C707125EE8E74E3221D3778;
		return (int32_t)L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_Subtract_TisIl2CppFullySharedGenericAny_m77EA314D255D1D8FD3C69638ED52F8D3BA5EC242_gshared (Il2CppFullySharedGenericAny* ___0_source, int32_t ___1_elementOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_tD9B75C3A90F5ADAE1D4CB286BF589C64D6F826CF = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		int32_t L_1 = ___1_elementOffset;
		uint32_t L_2 = SizeOf_T_tD9B75C3A90F5ADAE1D4CB286BF589C64D6F826CF;
		return ((Il2CppFullySharedGenericAny*)il2cpp_codegen_subtract((intptr_t)L_0, ((intptr_t)il2cpp_codegen_multiply(L_1, ((intptr_t)L_2)))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_Subtract_TisIl2CppFullySharedGenericAny_m0EB09BFC30B49EDB24334A3E6BA35483C93558C6_gshared (Il2CppFullySharedGenericAny* ___0_source, intptr_t ___1_elementOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t3B1E77C02DB4AAB58F90922E3099FF9917049F54 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		intptr_t L_1 = ___1_elementOffset;
		uint32_t L_2 = SizeOf_T_t3B1E77C02DB4AAB58F90922E3099FF9917049F54;
		return ((Il2CppFullySharedGenericAny*)il2cpp_codegen_subtract((intptr_t)L_0, ((intptr_t)il2cpp_codegen_multiply(L_1, (int32_t)L_2))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void* Unsafe_Subtract_TisIl2CppFullySharedGenericAny_m69F91E5F01FC5A2511D3A27E7F5C70FC1EB7CFC4_gshared (void* ___0_source, int32_t ___1_elementOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t6431A832C0FE9B48DEC156C6F433FD40910285B7 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_elementOffset;
		uint32_t L_2 = SizeOf_T_t6431A832C0FE9B48DEC156C6F433FD40910285B7;
		return ((void*)il2cpp_codegen_subtract((intptr_t)L_0, ((intptr_t)il2cpp_codegen_multiply(L_1, ((intptr_t)L_2)))));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Unsafe_SubtractByteOffset_TisIl2CppFullySharedGenericAny_mE7292258E19E7321B0C18390E6F2EEE56137A932_gshared (Il2CppFullySharedGenericAny* ___0_source, intptr_t ___1_byteOffset, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_source;
		intptr_t L_1 = ___1_byteOffset;
		return ((Il2CppFullySharedGenericAny*)il2cpp_codegen_subtract((intptr_t)L_0, L_1));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericStruct* Unsafe_Unbox_TisIl2CppFullySharedGenericStruct_m88B49D41AEB0FFFE9D8A4DAE3E5273D9BE77D436_gshared (RuntimeObject* ___0_box, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		RuntimeObject* L_0 = ___0_box;
		return ((Il2CppFullySharedGenericStruct*)UnBox(L_0, il2cpp_rgctx_data(method->rgctx_data, 0)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_Write_TisIl2CppFullySharedGenericAny_mF4407B2C4BB068E88E90A6F3B2E5ED86835B96B5_gshared (void* ___0_destination, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t987E376B027C09DB1CFE376088838CD47A845373 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_t987E376B027C09DB1CFE376088838CD47A845373);
	{
		void* L_0 = ___0_destination;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_value : &___1_value), SizeOf_T_t987E376B027C09DB1CFE376088838CD47A845373);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_0, L_1, SizeOf_T_t987E376B027C09DB1CFE376088838CD47A845373);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->rgctx_data, 0), (void**)(Il2CppFullySharedGenericAny*)L_0, (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_WriteUnaligned_TisIl2CppFullySharedGenericAny_m88218FF270F5A4591BBB29925EB495E7EE79A85A_gshared (uint8_t* ___0_destination, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t90B1ABDBB86D5A9C31296CDFE2FF785AA17940C2 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_t90B1ABDBB86D5A9C31296CDFE2FF785AA17940C2);
	{
		uint8_t* L_0 = ___0_destination;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_value : &___1_value), SizeOf_T_t90B1ABDBB86D5A9C31296CDFE2FF785AA17940C2);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_0, L_1, SizeOf_T_t90B1ABDBB86D5A9C31296CDFE2FF785AA17940C2);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->rgctx_data, 0), (void**)(Il2CppFullySharedGenericAny*)L_0, (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_WriteUnaligned_TisIl2CppFullySharedGenericAny_mAD6CD3DA0E8F4B38033235CACBD8838E554661F6_gshared (void* ___0_destination, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t5D35E9AA45F3F017961C19D66341B8F66F95CC5F = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_t5D35E9AA45F3F017961C19D66341B8F66F95CC5F);
	{
		void* L_0 = ___0_destination;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_value : &___1_value), SizeOf_T_t5D35E9AA45F3F017961C19D66341B8F66F95CC5F);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_0, L_1, SizeOf_T_t5D35E9AA45F3F017961C19D66341B8F66F95CC5F);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->rgctx_data, 0), (void**)(Il2CppFullySharedGenericAny*)L_0, (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m77D1ED5AB6CFA9B6FF9CC1CFB2B72783A7828BF5_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, uint8_t ___0_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t V_0 = 0;
	void* V_1 = NULL;
	bool V_2 = false;
	{
		int32_t L_0;
		L_0 = UnsafeUtility_SizeOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m97C7D5E5DE74DC60A0ECAA914830BEDF2C46ACAA_inline(il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_0;
		int32_t L_1 = __this->___Length;
		int32_t L_2 = V_0;
		UnsafeAppendBuffer_SetCapacity_mF80BF3DD388E819E52586A458C10606333611269(__this, ((int32_t)il2cpp_codegen_add(L_1, L_2)), NULL);
		uint8_t* L_3 = __this->___Ptr;
		int32_t L_4 = __this->___Length;
		V_1 = (void*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_3, L_4));
		void* L_5 = V_1;
		int32_t L_6;
		L_6 = UnsafeUtility_AlignOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mB152D3C658FB48DEDB6E662EEB0C91018E7F2857(il2cpp_rgctx_method(method->rgctx_data, 1));
		bool L_7;
		L_7 = CollectionHelper_IsAligned_m4761669B9084B892256349C0FF27DBF494DA9AE9((uint64_t)((int64_t)(uint64_t)((uintptr_t)(intptr_t)L_5)), L_6, NULL);
		V_2 = L_7;
		bool L_8 = V_2;
		if (!L_8)
		{
			goto IL_003f;
		}
	}
	{
		void* L_9 = V_1;
		UnsafeUtility_CopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mD5A26D4923C66071413642D33867560D087F543F_inline((&___0_value), L_9, il2cpp_rgctx_method(method->rgctx_data, 3));
		goto IL_004b;
	}

IL_003f:
	{
		void* L_10 = V_1;
		int32_t L_11 = V_0;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177(L_10, (void*)((uintptr_t)(&___0_value)), ((int64_t)L_11), NULL);
	}

IL_004b:
	{
		int32_t L_12 = __this->___Length;
		int32_t L_13 = V_0;
		__this->___Length = ((int32_t)il2cpp_codegen_add(L_12, L_13));
		return;
	}
}
IL2CPP_EXTERN_C  void UnsafeAppendBuffer_Add_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m77D1ED5AB6CFA9B6FF9CC1CFB2B72783A7828BF5_AdjustorThunk (RuntimeObject* __this, uint8_t ___0_value, const RuntimeMethod* method)
{
	UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*>(__this + _offset);
	UnsafeAppendBuffer_Add_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m77D1ED5AB6CFA9B6FF9CC1CFB2B72783A7828BF5(_thisAdjusted, ___0_value, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t V_0 = 0;
	void* V_1 = NULL;
	bool V_2 = false;
	{
		int32_t L_0;
		L_0 = UnsafeUtility_SizeOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mED481D505BF43CBD96972069EDD4E3509BE84931_inline(il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_0;
		int32_t L_1 = __this->___Length;
		int32_t L_2 = V_0;
		UnsafeAppendBuffer_SetCapacity_mF80BF3DD388E819E52586A458C10606333611269(__this, ((int32_t)il2cpp_codegen_add(L_1, L_2)), NULL);
		uint8_t* L_3 = __this->___Ptr;
		int32_t L_4 = __this->___Length;
		V_1 = (void*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_3, L_4));
		void* L_5 = V_1;
		int32_t L_6;
		L_6 = UnsafeUtility_AlignOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m274F833CD001C63E56A22D8F42C76CE8C6CC39DF(il2cpp_rgctx_method(method->rgctx_data, 1));
		bool L_7;
		L_7 = CollectionHelper_IsAligned_m4761669B9084B892256349C0FF27DBF494DA9AE9((uint64_t)((int64_t)(uint64_t)((uintptr_t)(intptr_t)L_5)), L_6, NULL);
		V_2 = L_7;
		bool L_8 = V_2;
		if (!L_8)
		{
			goto IL_003f;
		}
	}
	{
		void* L_9 = V_1;
		UnsafeUtility_CopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m934C9E3CBBFA04ABEF4528E565C300B5DCF8C2D1_inline((&___0_value), L_9, il2cpp_rgctx_method(method->rgctx_data, 3));
		goto IL_004b;
	}

IL_003f:
	{
		void* L_10 = V_1;
		int32_t L_11 = V_0;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177(L_10, (void*)((uintptr_t)(&___0_value)), ((int64_t)L_11), NULL);
	}

IL_004b:
	{
		int32_t L_12 = __this->___Length;
		int32_t L_13 = V_0;
		__this->___Length = ((int32_t)il2cpp_codegen_add(L_12, L_13));
		return;
	}
}
IL2CPP_EXTERN_C  void UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_AdjustorThunk (RuntimeObject* __this, int32_t ___0_value, const RuntimeMethod* method)
{
	UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*>(__this + _offset);
	UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1(_thisAdjusted, ___0_value, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mDDC9C1D1487BB5B9B776AD9F9B6581F838DA5C09_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, Il2CppFullySharedGenericStruct ___0_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t V_0 = 0;
	void* V_1 = NULL;
	bool V_2 = false;
	{
		int32_t L_0;
		L_0 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_0;
		int32_t L_1 = __this->___Length;
		int32_t L_2 = V_0;
		UnsafeAppendBuffer_SetCapacity_mF80BF3DD388E819E52586A458C10606333611269(__this, ((int32_t)il2cpp_codegen_add(L_1, L_2)), NULL);
		uint8_t* L_3 = __this->___Ptr;
		int32_t L_4 = __this->___Length;
		V_1 = (void*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_3, L_4));
		void* L_5 = V_1;
		int32_t L_6;
		L_6 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(il2cpp_rgctx_method(method->rgctx_data, 1));
		bool L_7;
		L_7 = CollectionHelper_IsAligned_m4761669B9084B892256349C0FF27DBF494DA9AE9((uint64_t)((int64_t)(uint64_t)((uintptr_t)(intptr_t)L_5)), L_6, NULL);
		V_2 = L_7;
		bool L_8 = V_2;
		if (!L_8)
		{
			goto IL_003f;
		}
	}
	{
		void* L_9 = V_1;
		((  void (*) (Il2CppFullySharedGenericStruct*, void*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)))((Il2CppFullySharedGenericStruct*)___0_value, L_9, il2cpp_rgctx_method(method->rgctx_data, 3));
		goto IL_004b;
	}

IL_003f:
	{
		void* L_10 = V_1;
		int32_t L_11 = V_0;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177(L_10, (void*)((uintptr_t)(Il2CppFullySharedGenericStruct*)___0_value), ((int64_t)L_11), NULL);
	}

IL_004b:
	{
		int32_t L_12 = __this->___Length;
		int32_t L_13 = V_0;
		__this->___Length = ((int32_t)il2cpp_codegen_add(L_12, L_13));
		return;
	}
}
IL2CPP_EXTERN_C  void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mDDC9C1D1487BB5B9B776AD9F9B6581F838DA5C09_AdjustorThunk (RuntimeObject* __this, Il2CppFullySharedGenericStruct ___0_value, const RuntimeMethod* method)
{
	UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*>(__this + _offset);
	UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mDDC9C1D1487BB5B9B776AD9F9B6581F838DA5C09(_thisAdjusted, ___0_value, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mBC28FDDA775CF46EBFF35FACA19AB97DA55D7C5C_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___0_value, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	{
		int32_t L_0;
		L_0 = ((  int32_t (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___0_value), il2cpp_rgctx_method(method->rgctx_data, 1));
		UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1(__this, L_0, UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_RuntimeMethod_var);
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_1 = ___0_value;
		void* L_2;
		L_2 = ((  void* (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)))(L_1, il2cpp_rgctx_method(method->rgctx_data, 3));
		int32_t L_3;
		L_3 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)))(il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_4;
		L_4 = ((  int32_t (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___0_value), il2cpp_rgctx_method(method->rgctx_data, 1));
		UnsafeAppendBuffer_Add_m1CE98EFF1C7CAFEDCDE896B21530D3B2A3311D25(__this, L_2, ((int32_t)il2cpp_codegen_multiply(L_3, L_4)), NULL);
		return;
	}
}
IL2CPP_EXTERN_C  void UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mBC28FDDA775CF46EBFF35FACA19AB97DA55D7C5C_AdjustorThunk (RuntimeObject* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___0_value, const RuntimeMethod* method)
{
	UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*>(__this + _offset);
	UnsafeAppendBuffer_Add_TisIl2CppFullySharedGenericStruct_mBC28FDDA775CF46EBFF35FACA19AB97DA55D7C5C(_thisAdjusted, ___0_value, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_AddArray_TisIl2CppFullySharedGenericStruct_m5627B5F2DCE95940F3F9DFF77A97789E2CCD5E57_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, void* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	bool V_0 = false;
	{
		int32_t L_0 = ___1_length;
		UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1(__this, L_0, UnsafeAppendBuffer_Add_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m115C1E10CFF2E5179C099DDD59C5779D0863C1C1_RuntimeMethod_var);
		int32_t L_1 = ___1_length;
		V_0 = (bool)((!(((uint32_t)L_1) <= ((uint32_t)0)))? 1 : 0);
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_0020;
		}
	}
	{
		void* L_3 = ___0_ptr;
		int32_t L_4 = ___1_length;
		int32_t L_5;
		L_5 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(il2cpp_rgctx_method(method->rgctx_data, 0));
		UnsafeAppendBuffer_Add_m1CE98EFF1C7CAFEDCDE896B21530D3B2A3311D25(__this, L_3, ((int32_t)il2cpp_codegen_multiply(L_4, L_5)), NULL);
	}

IL_0020:
	{
		return;
	}
}
IL2CPP_EXTERN_C  void UnsafeAppendBuffer_AddArray_TisIl2CppFullySharedGenericStruct_m5627B5F2DCE95940F3F9DFF77A97789E2CCD5E57_AdjustorThunk (RuntimeObject* __this, void* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*>(__this + _offset);
	UnsafeAppendBuffer_AddArray_TisIl2CppFullySharedGenericStruct_m5627B5F2DCE95940F3F9DFF77A97789E2CCD5E57(_thisAdjusted, ___0_ptr, ___1_length, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeAppendBuffer_Pop_TisIl2CppFullySharedGenericStruct_m504B53F43031CB15FF39BBBA94BE052B3E33D1A9_gshared (UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* __this, Il2CppFullySharedGenericStruct* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 3));
	const Il2CppFullySharedGenericStruct L_11 = alloca(SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
	const Il2CppFullySharedGenericStruct L_16 = L_11;
	const Il2CppFullySharedGenericStruct L_17 = L_11;
	int32_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	Il2CppFullySharedGenericStruct V_4 = alloca(SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
	memset(V_4, 0, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
	bool V_5 = false;
	Il2CppFullySharedGenericStruct V_6 = alloca(SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
	memset(V_6, 0, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
	{
		int32_t L_0;
		L_0 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_0;
		uint8_t* L_1 = __this->___Ptr;
		V_1 = ((int64_t)(uint64_t)((uintptr_t)(intptr_t)L_1));
		int32_t L_2 = __this->___Length;
		V_2 = ((int64_t)L_2);
		int64_t L_3 = V_1;
		int64_t L_4 = V_2;
		int32_t L_5 = V_0;
		V_3 = ((int64_t)il2cpp_codegen_subtract(((int64_t)il2cpp_codegen_add(L_3, L_4)), ((int64_t)L_5)));
		int64_t L_6 = V_3;
		int32_t L_7;
		L_7 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(il2cpp_rgctx_method(method->rgctx_data, 1));
		bool L_8;
		L_8 = CollectionHelper_IsAligned_m4761669B9084B892256349C0FF27DBF494DA9AE9((uint64_t)L_6, L_7, NULL);
		V_5 = L_8;
		bool L_9 = V_5;
		if (!L_9)
		{
			goto IL_003b;
		}
	}
	{
		int64_t L_10 = V_3;
		InvokerActionInvoker3< void*, int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, (void*)((uintptr_t)L_10), 0, (Il2CppFullySharedGenericStruct*)L_11);
		il2cpp_codegen_memcpy(V_4, L_11, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
		goto IL_0048;
	}

IL_003b:
	{
		int64_t L_12 = V_3;
		int32_t L_13 = V_0;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)((uintptr_t)(Il2CppFullySharedGenericStruct*)V_4), (void*)((uintptr_t)L_12), ((int64_t)L_13), NULL);
	}

IL_0048:
	{
		int32_t L_14 = __this->___Length;
		int32_t L_15 = V_0;
		__this->___Length = ((int32_t)il2cpp_codegen_subtract(L_14, L_15));
		il2cpp_codegen_memcpy(L_16, V_4, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
		il2cpp_codegen_memcpy(V_6, L_16, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
		goto IL_005c;
	}

IL_005c:
	{
		il2cpp_codegen_memcpy(L_17, V_6, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
		il2cpp_codegen_memcpy(il2cppRetVal, L_17, SizeOf_T_t46B54E1394E86F4EC59651D45B3DFBD912AF848E);
		return;
	}
}
IL2CPP_EXTERN_C  void UnsafeAppendBuffer_Pop_TisIl2CppFullySharedGenericStruct_m504B53F43031CB15FF39BBBA94BE052B3E33D1A9_AdjustorThunk (RuntimeObject* __this, Il2CppFullySharedGenericStruct* il2cppRetVal, const RuntimeMethod* method)
{
	UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<UnsafeAppendBuffer_t9EC160EA10485CD9DD96EDCBCEA06C7BCEF81500*>(__this + _offset);
	UnsafeAppendBuffer_Pop_TisIl2CppFullySharedGenericStruct_m504B53F43031CB15FF39BBBA94BE052B3E33D1A9(_thisAdjusted, il2cppRetVal, method);
	return;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_m4590CF7239319F64DD1DE7F1E5CF341CD5A88678_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* ___1_src, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___2_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* L_2 = ___1_src;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_3 = ___2_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233*, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_m20D6C46671CDF2A0117036841F76C9EA2E89765B_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* ___1_src, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___2_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* L_2 = ___1_src;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_3 = ___2_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1*, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_mBB3BE09088AA14350A4E99D0FA9ACE372092D649_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___2_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_3 = ___2_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_mDFF41C29FE9637F59BA78CB79E38512A3CF09AC7_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, int32_t ___2_mipIndex, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___3_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		int32_t L_3 = ___2_mipIndex;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_4 = ___3_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_mFC4D980DAF673130B0757D1BF0C74ECC72AA8C0E_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* ___1_src, int32_t ___2_size, int32_t ___3_offset, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___4_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* L_2 = ___1_src;
		int32_t L_3 = ___2_size;
		int32_t L_4 = ___3_offset;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_5 = ___4_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233*, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_m1797A693E3B4DD59699F15D1703EA6B1A3490A57_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* ___1_src, int32_t ___2_size, int32_t ___3_offset, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___4_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* L_2 = ___1_src;
		int32_t L_3 = ___2_size;
		int32_t L_4 = ___3_offset;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_5 = ___4_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1*, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_mA413BB50721484FE5C2F38E17C1C1E3F3B3D4CB3_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, int32_t ___2_mipIndex, int32_t ___3_dstFormat, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___4_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		int32_t L_3 = ___2_mipIndex;
		int32_t L_4 = ___3_dstFormat;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_5 = ___4_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_mC224093E7F4B147F2A1F86C804DDBBE19384AECB_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, int32_t ___2_mipIndex, int32_t ___3_dstFormat, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___4_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		int32_t L_3 = ___2_mipIndex;
		int32_t L_4 = ___3_dstFormat;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_5 = ___4_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_m3DB621F402102565ADA22FE7821F8684ACED6C87_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, int32_t ___2_mipIndex, int32_t ___3_x, int32_t ___4_width, int32_t ___5_y, int32_t ___6_height, int32_t ___7_z, int32_t ___8_depth, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___9_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		int32_t L_3 = ___2_mipIndex;
		int32_t L_4 = ___3_x;
		int32_t L_5 = ___4_width;
		int32_t L_6 = ___5_y;
		int32_t L_7 = ___6_height;
		int32_t L_8 = ___7_z;
		int32_t L_9 = ___8_depth;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_10 = ___9_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, L_6, L_7, L_8, L_9, L_10, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_m076CC5B64DE819DBAF372FE886BA2B90CBD3BC31_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, int32_t ___2_mipIndex, int32_t ___3_x, int32_t ___4_width, int32_t ___5_y, int32_t ___6_height, int32_t ___7_z, int32_t ___8_depth, int32_t ___9_dstFormat, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___10_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		int32_t L_3 = ___2_mipIndex;
		int32_t L_4 = ___3_x;
		int32_t L_5 = ___4_width;
		int32_t L_6 = ___5_y;
		int32_t L_7 = ___6_height;
		int32_t L_8 = ___7_z;
		int32_t L_9 = ___8_depth;
		int32_t L_10 = ___9_dstFormat;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_11 = ___10_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, L_6, L_7, L_8, L_9, L_10, L_11, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_RequestAsyncReadbackIntoNativeArray_TisIl2CppFullySharedGenericStruct_m6CE123F7CF5137595A8A743B5AF17BD8D2C3F66B_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* ___0_output, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_src, int32_t ___2_mipIndex, int32_t ___3_x, int32_t ___4_width, int32_t ___5_y, int32_t ___6_height, int32_t ___7_z, int32_t ___8_depth, int32_t ___9_dstFormat, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* ___10_callback, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_1 = ___0_output;
		Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* L_2 = ___1_src;
		int32_t L_3 = ___2_mipIndex;
		int32_t L_4 = ___3_x;
		int32_t L_5 = ___4_width;
		int32_t L_6 = ___5_y;
		int32_t L_7 = ___6_height;
		int32_t L_8 = ___7_z;
		int32_t L_9 = ___8_depth;
		int32_t L_10 = ___9_dstFormat;
		Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F* L_11 = ___10_callback;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700*, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, Action_1_t6AC29B34E68BC53AA807670D868CBB59CD5D995F*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, L_6, L_7, L_8, L_9, L_10, L_11, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_m4612A502BC75360BA91CFDF3E61DBCB1D75E90CB_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* ___0_buffer, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* ___1_data, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* L_1 = ___0_buffer;
		List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* L_2 = ___1_data;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233*, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_m27C6E5B0D3DF6CFB5CB1474C6AE92BD28D641399_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* ___0_buffer, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___1_data, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* L_1 = ___0_buffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_2 = ___1_data;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_m9DF8E236C9AD375EAB748CC5715F23ABBEE6BA75_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* ___0_buffer, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* ___1_data, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* L_1 = ___0_buffer;
		List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* L_2 = ___1_data;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1*, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_m3F9A4F561905D138501C3EF53CAF41EBF524A8E2_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* ___0_buffer, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___1_data, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* L_1 = ___0_buffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_2 = ___1_data;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_mE27938896498F85F95486585EB3BB8F54DBEF3D9_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* ___0_buffer, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* ___1_data, int32_t ___2_managedBufferStartIndex, int32_t ___3_graphicsBufferStartIndex, int32_t ___4_count, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* L_1 = ___0_buffer;
		List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* L_2 = ___1_data;
		int32_t L_3 = ___2_managedBufferStartIndex;
		int32_t L_4 = ___3_graphicsBufferStartIndex;
		int32_t L_5 = ___4_count;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233*, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4*, int32_t, int32_t, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_mD59E7951302BED8F22E9A1C0D3E3EE37ADEFA4AC_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* ___0_buffer, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___1_data, int32_t ___2_nativeBufferStartIndex, int32_t ___3_graphicsBufferStartIndex, int32_t ___4_count, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233* L_1 = ___0_buffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_2 = ___1_data;
		int32_t L_3 = ___2_nativeBufferStartIndex;
		int32_t L_4 = ___3_graphicsBufferStartIndex;
		int32_t L_5 = ___4_count;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, ComputeBuffer_t51EADA9015EBCC1B982C5584E9AB2734415A8233*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, int32_t, int32_t, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_m146FB8A0C1686793072BF6F4C14A4F38E29FD925_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* ___0_buffer, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* ___1_data, int32_t ___2_managedBufferStartIndex, int32_t ___3_graphicsBufferStartIndex, int32_t ___4_count, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* L_1 = ___0_buffer;
		List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4* L_2 = ___1_data;
		int32_t L_3 = ___2_managedBufferStartIndex;
		int32_t L_4 = ___3_graphicsBufferStartIndex;
		int32_t L_5 = ___4_count;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1*, List_1_tE5211AFE92BF26309D7C5814A29544E9EF496FE4*, int32_t, int32_t, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeCommandBuffer_SetBufferData_TisIl2CppFullySharedGenericStruct_m605037FF731BED168BA65C7E3A07F6B48A2F76C5_gshared (UnsafeCommandBuffer_tDE6BB2FE234DC7453CA682AB275888E9E35F22F2* __this, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* ___0_buffer, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___1_data, int32_t ___2_nativeBufferStartIndex, int32_t ___3_graphicsBufferStartIndex, int32_t ___4_count, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* L_0 = ((BaseCommandBuffer_tD67BB9B3F740537BD3F3A96FA17D06E6C3BFDC06*)__this)->___m_WrappedCommandBuffer;
		GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1* L_1 = ___0_buffer;
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_2 = ___1_data;
		int32_t L_3 = ___2_nativeBufferStartIndex;
		int32_t L_4 = ___3_graphicsBufferStartIndex;
		int32_t L_5 = ___4_count;
		NullCheck(L_0);
		((  void (*) (CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7*, GraphicsBuffer_t91FACD3CD78588C25C361C453D1A2FE055EC4AF1*, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, int32_t, int32_t, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_0, L_1, L_2, L_3, L_4, L_5, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool UnsafeListExtensions_ArraysEqual_TisIl2CppFullySharedGenericStruct_m26517D1B620B6BB143F8CDBCBC9299D6C9AA512F_gshared (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 ___0_container, UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6* ___1_other, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t36936CBBA76E77B94AE3777BAF4D0FDDB161EC12 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 5));
	void* L_10 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data(method->rgctx_data, 5)));
	const Il2CppFullySharedGenericStruct L_5 = alloca(SizeOf_T_t36936CBBA76E77B94AE3777BAF4D0FDDB161EC12);
	const Il2CppFullySharedGenericStruct L_9 = alloca(SizeOf_T_t36936CBBA76E77B94AE3777BAF4D0FDDB161EC12);
	bool V_0 = false;
	bool V_1 = false;
	int32_t V_2 = 0;
	bool V_3 = false;
	Il2CppFullySharedGenericStruct V_4 = alloca(SizeOf_T_t36936CBBA76E77B94AE3777BAF4D0FDDB161EC12);
	memset(V_4, 0, SizeOf_T_t36936CBBA76E77B94AE3777BAF4D0FDDB161EC12);
	UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 V_5;
	memset((&V_5), 0, sizeof(V_5));
	bool V_6 = false;
	{
		int32_t L_0;
		L_0 = ((  int32_t (*) (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___0_container), il2cpp_rgctx_method(method->rgctx_data, 1));
		UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6* L_1 = ___1_other;
		int32_t L_2;
		L_2 = ((  int32_t (*) (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))(L_1, il2cpp_rgctx_method(method->rgctx_data, 1));
		V_0 = (bool)((((int32_t)((((int32_t)L_0) == ((int32_t)L_2))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_3 = V_0;
		if (!L_3)
		{
			goto IL_001b;
		}
	}
	{
		V_1 = (bool)0;
		goto IL_006e;
	}

IL_001b:
	{
		V_2 = 0;
		goto IL_0057;
	}

IL_001f:
	{
		int32_t L_4 = V_2;
		InvokerActionInvoker2< int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)), il2cpp_rgctx_method(method->rgctx_data, 4), (&___0_container), L_4, (Il2CppFullySharedGenericStruct*)L_5);
		il2cpp_codegen_memcpy(V_4, L_5, SizeOf_T_t36936CBBA76E77B94AE3777BAF4D0FDDB161EC12);
		UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6* L_6 = ___1_other;
		UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 L_7 = (*(UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6*)L_6);
		V_5 = L_7;
		int32_t L_8 = V_2;
		InvokerActionInvoker2< int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)), il2cpp_rgctx_method(method->rgctx_data, 4), (&V_5), L_8, (Il2CppFullySharedGenericStruct*)L_9);
		bool L_11;
		L_11 = ConstrainedFuncInvoker1< bool, Il2CppFullySharedGenericStruct >::Invoke(il2cpp_rgctx_data(method->rgctx_data, 5), il2cpp_rgctx_method(method->rgctx_data, 7), L_10, (void*)(Il2CppFullySharedGenericStruct*)V_4, L_9);
		V_3 = (bool)((((int32_t)L_11) == ((int32_t)0))? 1 : 0);
		bool L_12 = V_3;
		if (!L_12)
		{
			goto IL_0052;
		}
	}
	{
		V_1 = (bool)0;
		goto IL_006e;
	}

IL_0052:
	{
		int32_t L_13 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_13, 1));
	}

IL_0057:
	{
		int32_t L_14 = V_2;
		int32_t L_15;
		L_15 = ((  int32_t (*) (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___0_container), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_6 = (bool)((((int32_t)((((int32_t)L_14) == ((int32_t)L_15))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_16 = V_6;
		if (L_16)
		{
			goto IL_001f;
		}
	}
	{
		V_1 = (bool)1;
		goto IL_006e;
	}

IL_006e:
	{
		bool L_17 = V_1;
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool UnsafeListExtensions_Contains_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericAny_m849EF25437A5D844E398509A94A164CA1BD07D7A_gshared (ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839 ___0_list, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_U_t71E8B5E5C2FEAD5E433A30BDC27D9E9F6C907692 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_U_t71E8B5E5C2FEAD5E433A30BDC27D9E9F6C907692);
	bool V_0 = false;
	{
		ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839 L_0 = ___0_list;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_value : &___1_value), SizeOf_U_t71E8B5E5C2FEAD5E433A30BDC27D9E9F6C907692);
		int32_t L_2;
		L_2 = InvokerFuncInvoker2< int32_t, ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_1: *(void**)L_1));
		V_0 = (bool)((((int32_t)((((int32_t)L_2) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0011;
	}

IL_0011:
	{
		bool L_3 = V_0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool UnsafeListExtensions_Contains_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericAny_m9A04B9F17C4745874BFCBA06B0CA62B3C1BD8618_gshared (ReadOnly_t620C834D6C54D781225D753876F232E09821956C ___0_list, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_U_tDC75599B528D5C6C23F210DE1975466A48F27F8C = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_U_tDC75599B528D5C6C23F210DE1975466A48F27F8C);
	bool V_0 = false;
	{
		ReadOnly_t620C834D6C54D781225D753876F232E09821956C L_0 = ___0_list;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_value : &___1_value), SizeOf_U_tDC75599B528D5C6C23F210DE1975466A48F27F8C);
		int32_t L_2;
		L_2 = InvokerFuncInvoker2< int32_t, ReadOnly_t620C834D6C54D781225D753876F232E09821956C, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_1: *(void**)L_1));
		V_0 = (bool)((((int32_t)((((int32_t)L_2) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0011;
	}

IL_0011:
	{
		bool L_3 = V_0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool UnsafeListExtensions_Contains_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericAny_mEA66444086816FFF6C02FDEDF064DC92D43ABB07_gshared (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 ___0_list, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_U_t79ED4F2F667B96B1F466267D4E93F6567EE08001 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_U_t79ED4F2F667B96B1F466267D4E93F6567EE08001);
	bool V_0 = false;
	{
		UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 L_0 = ___0_list;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_value : &___1_value), SizeOf_U_t79ED4F2F667B96B1F466267D4E93F6567EE08001);
		int32_t L_2;
		L_2 = InvokerFuncInvoker2< int32_t, UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_1: *(void**)L_1));
		V_0 = (bool)((((int32_t)((((int32_t)L_2) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		goto IL_0011;
	}

IL_0011:
	{
		bool L_3 = V_0;
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UnsafeListExtensions_IndexOf_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericAny_m4C0405202053EC610D5814D16972342C44B04015_gshared (ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839 ___0_list, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_U_t527490260DD61FB68E8B24509583B4E5B8004E85 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_4 = alloca(SizeOf_U_t527490260DD61FB68E8B24509583B4E5B8004E85);
	int32_t V_0 = 0;
	{
		ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839 L_0 = ___0_list;
		Il2CppFullySharedGenericStruct* L_1 = L_0.___Ptr;
		ParallelReader_tE9E5B90E22A4BC77DB4B327155E0A9F071FC4839 L_2 = ___0_list;
		int32_t L_3 = L_2.___Length;
		il2cpp_codegen_memcpy(L_4, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 2)) ? ___1_value : &___1_value), SizeOf_U_t527490260DD61FB68E8B24509583B4E5B8004E85);
		int32_t L_5;
		L_5 = InvokerFuncInvoker3< int32_t, void*, int32_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)), il2cpp_rgctx_method(method->rgctx_data, 3), NULL, (void*)L_1, L_3, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 2)) ? L_4: *(void**)L_4));
		V_0 = L_5;
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_6 = V_0;
		return L_6;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UnsafeListExtensions_IndexOf_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericAny_m666B6958EA2FFC68FF7ED5C8350CE4F72D717323_gshared (ReadOnly_t620C834D6C54D781225D753876F232E09821956C ___0_list, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_U_t8A2813E5F32C60A802660497A33599DED0C1E01F = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_4 = alloca(SizeOf_U_t8A2813E5F32C60A802660497A33599DED0C1E01F);
	int32_t V_0 = 0;
	{
		ReadOnly_t620C834D6C54D781225D753876F232E09821956C L_0 = ___0_list;
		Il2CppFullySharedGenericStruct* L_1 = L_0.___Ptr;
		ReadOnly_t620C834D6C54D781225D753876F232E09821956C L_2 = ___0_list;
		int32_t L_3 = L_2.___Length;
		il2cpp_codegen_memcpy(L_4, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 2)) ? ___1_value : &___1_value), SizeOf_U_t8A2813E5F32C60A802660497A33599DED0C1E01F);
		int32_t L_5;
		L_5 = InvokerFuncInvoker3< int32_t, void*, int32_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)), il2cpp_rgctx_method(method->rgctx_data, 3), NULL, (void*)L_1, L_3, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 2)) ? L_4: *(void**)L_4));
		V_0 = L_5;
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_6 = V_0;
		return L_6;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t UnsafeListExtensions_IndexOf_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericAny_m57EF2104D6B01A3B93003ABF811294D496610604_gshared (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 ___0_list, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_U_t54CFCA7844B58AFDB181E954D6BBD75E1E48ACF7 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 4));
	const Il2CppFullySharedGenericAny L_3 = alloca(SizeOf_U_t54CFCA7844B58AFDB181E954D6BBD75E1E48ACF7);
	int32_t V_0 = 0;
	{
		UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6 L_0 = ___0_list;
		Il2CppFullySharedGenericStruct* L_1 = L_0.___Ptr;
		int32_t L_2;
		L_2 = ((  int32_t (*) (UnsafeList_1_t3A26A222433F7993EC942046A500D6EA3DCB97E6*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)))((&___0_list), il2cpp_rgctx_method(method->rgctx_data, 2));
		il2cpp_codegen_memcpy(L_3, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)) ? ___1_value : &___1_value), SizeOf_U_t54CFCA7844B58AFDB181E954D6BBD75E1E48ACF7);
		int32_t L_4;
		L_4 = InvokerFuncInvoker3< int32_t, void*, int32_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 5)), il2cpp_rgctx_method(method->rgctx_data, 5), NULL, (void*)L_1, L_2, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)) ? L_3: *(void**)L_3));
		V_0 = L_4;
		goto IL_0017;
	}

IL_0017:
	{
		int32_t L_5 = V_0;
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m7F02287D50FD674468F0A855682F2C07E8CF8C3A_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mCBA4B05BA324F08F46C24FAEAF26B6FA71706FDC(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCCBA9B456E509039F0C37BA72EFD6D0CDAF16BB6_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEA2D219466CA70D0772888D0B2AA5BDF95B3D7B0(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m1F07F89C00AD478D5CB75EC22799BABEEB9D5A52_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m618DEF61FF37029CC5BB378A9DF74D17BF431F06(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mE1E3FA9B9A3B76CD13201F8AF45C33A2AA83612E_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m1FF8E4DB8A087F7A862C15EEC0913C3FCD81FFF5(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m20A60CA74A0B844D0A309565CCBB0849BB5A5831_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m4F68FC2BD5F10999A62EE62207C4B8B4E57489D1(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mC5DB96096F58345413CB2300A12310634E7D4AB1_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mBBBCD7A3795516F652E2A209921D9F365FEF117D(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mB3FAEAB9C60EB3A7B2966293CED32A184224B5C2_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mFCFCB281237C3FD181B82326F1CDC74ECDC26835(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFC6AE4DAC01DFDFF29C1E53E06F14FB0B0D66206_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m28DB190FC25AC6811E2F609C118B25148772E8ED(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m06B252B4A30C4FBFCC2814BE89ED44E4BF844662_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mB677466F5AF7999905E08C12DA5F85B135C65DA2(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m88D8FAB7DC9FED19157163DE70F30347E1C77FCE_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mA6587978CB654E5F829A8E1413F71B7210CE52FB(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mAEC4C336956F04EC1DC3D94D60D85EB0015F5B47_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB151D38E73EEDFB7B82F1F6498C9906EB81D402A(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m62629C19185CA06E83F04EC7AE00FDFCACF04587_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m7CF1071B4128F92A069498179CE85F80CB5AEBE1(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m951C01373AAD44331FD5671F1217E4F92009C4A7_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mDDA0397F7F044E6209E9F8A2D66077E9E5CB485E(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m70C09EC3417214F644DA0D50C71EE09BA91C4A51_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m8BB02CE71160A2514C280DBDC8A5253F93E3689E(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDD38B740F2986420537A4FABB757116266FE03B9_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m47E7EA1FA34E44C4B7E215FA109EFC5813E93948(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDC2B55CE8A56A026D6A1FD2F6F5E078ADA4B492A_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE5DE2AA98BC9864F8C516BE7C30A490B373C35D7(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m589EF9519E089B9F7023A20737E33E6786A7BC66_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m20514B296368E3BD19C54B8257A916FCC7837FCB(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m0A1278502FF54669B7FEC7E091A74F45688D23B9_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mF58A503E41D28232BA45EF49B867720056B3C3EB(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m8802747A63F08F7229D7AD998621B29674AC1392_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m84BBCADD738D8E17724D100724F1173F24CC9D46(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mBAA6B72D2B45297384CE311773557353F2E031B8_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m92740FC50E0692378ABED6D572FE0AA5FFF18DFB(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericStruct_mD7F5F49BFAB2AEE159CED53DACA62E9374B0B063_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_AllocateHashMap_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m37354636A64DD078FDE80400E173603F4B55D4F0_gshared (int32_t ___0_length, int64_t ___1_bucketLength, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___2_label, UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** ___3_outBuf, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* V_2 = NULL;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = JobsUtility_get_ThreadIndexCount_m88A954344398143540618B35943F863B92465676(NULL);
		V_0 = L_0;
		uint32_t L_1 = sizeof(UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926);
		il2cpp_codegen_runtime_class_init_inline(Assert_tDC16963451AC4364803739B73A4477ADCB365863_il2cpp_TypeInfo_var);
		Assert_IsTrue_mE42C53B7220324D1FBAFB7AE48A7D8DD7796A663((bool)((((int32_t)((((int32_t)L_1) > ((int32_t)((int32_t)64)))? 1 : 0)) == ((int32_t)0))? 1 : 0), NULL);
		int32_t L_2 = V_0;
		V_1 = ((int32_t)il2cpp_codegen_add(((int32_t)64), ((int32_t)il2cpp_codegen_multiply(((int32_t)64), L_2))));
		int32_t L_3 = V_1;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4 = ___2_label;
		void* L_5;
		L_5 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(((int64_t)L_3), ((int32_t)64), L_4, NULL);
		V_2 = (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926*)L_5;
		int64_t L_6 = ___1_bucketLength;
		int64_t L_7;
		L_7 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_6, NULL);
		___1_bucketLength = L_7;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_8 = V_2;
		int32_t L_9 = ___0_length;
		NullCheck(L_8);
		L_8->___keyCapacity = L_9;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = V_2;
		int64_t L_11 = ___1_bucketLength;
		NullCheck(L_10);
		L_10->___bucketCapacityMask = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_11, ((int64_t)1))));
		int32_t L_12 = ___0_length;
		int64_t L_13 = ___1_bucketLength;
		int64_t L_14;
		L_14 = UnsafeParallelHashMapData_CalculateDataSize_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m350360D9B128D3CC31EB85A15C4801B0D0CBDED2(L_12, L_13, (&V_3), (&V_4), (&V_5), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_6 = L_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_15 = V_2;
		int64_t L_16 = V_6;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_17 = ___2_label;
		void* L_18;
		L_18 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_16, ((int32_t)64), L_17, NULL);
		NullCheck(L_15);
		L_15->___values = (uint8_t*)L_18;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_19 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_20 = V_2;
		NullCheck(L_20);
		uint8_t* L_21 = L_20->___values;
		int64_t L_22 = V_3;
		NullCheck(L_19);
		L_19->___keys = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_21, ((intptr_t)L_22)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_23 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = V_2;
		NullCheck(L_24);
		uint8_t* L_25 = L_24->___values;
		int64_t L_26 = V_4;
		NullCheck(L_23);
		L_23->___next = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_25, ((intptr_t)L_26)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_27 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = V_2;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___values;
		int64_t L_30 = V_5;
		NullCheck(L_27);
		L_27->___buckets = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_29, ((intptr_t)L_30)));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926** L_31 = ___3_outBuf;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_32 = V_2;
		*((intptr_t*)L_31) = (intptr_t)L_32;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mCBA4B05BA324F08F46C24FAEAF26B6FA71706FDC_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEA2D219466CA70D0772888D0B2AA5BDF95B3D7B0_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m618DEF61FF37029CC5BB378A9DF74D17BF431F06_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m1FF8E4DB8A087F7A862C15EEC0913C3FCD81FFF5_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m4F68FC2BD5F10999A62EE62207C4B8B4E57489D1_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mBBBCD7A3795516F652E2A209921D9F365FEF117D_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mFCFCB281237C3FD181B82326F1CDC74ECDC26835_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m28DB190FC25AC6811E2F609C118B25148772E8ED_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mB677466F5AF7999905E08C12DA5F85B135C65DA2_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mA6587978CB654E5F829A8E1413F71B7210CE52FB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB151D38E73EEDFB7B82F1F6498C9906EB81D402A_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(int32_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m7CF1071B4128F92A069498179CE85F80CB5AEBE1_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(int32_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mDDA0397F7F044E6209E9F8A2D66077E9E5CB485E_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(int64_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m8BB02CE71160A2514C280DBDC8A5253F93E3689E_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m47E7EA1FA34E44C4B7E215FA109EFC5813E93948_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE5DE2AA98BC9864F8C516BE7C30A490B373C35D7_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m20514B296368E3BD19C54B8257A916FCC7837FCB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(uint16_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mF58A503E41D28232BA45EF49B867720056B3C3EB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(uint32_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m84BBCADD738D8E17724D100724F1173F24CC9D46_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(uint32_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m92740FC50E0692378ABED6D572FE0AA5FFF18DFB_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(int32_t);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(uint32_t);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericStruct_m86C560D40D8AAEF07631BFB8D1698688602BFD91_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_TValue_tB73822E5C10E34F3A9A9B541102F7CAD7D84853E = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const uint32_t SizeOf_TKey_tDBC8C46C28C29C419CF2B4A775A1045FCC9C9F3D = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = SizeOf_TValue_tB73822E5C10E34F3A9A9B541102F7CAD7D84853E;
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = SizeOf_TKey_tDBC8C46C28C29C419CF2B4A775A1045FCC9C9F3D;
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t UnsafeParallelHashMapData_CalculateDataSize_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m350360D9B128D3CC31EB85A15C4801B0D0CBDED2_gshared (int32_t ___0_length, int64_t ___1_bucketLength, int64_t* ___2_keyOffset, int64_t* ___3_nextOffset, int64_t* ___4_bucketOffset, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	int64_t V_4 = 0;
	int64_t V_5 = 0;
	int64_t V_6 = 0;
	int64_t V_7 = 0;
	int64_t V_8 = 0;
	{
		uint32_t L_0 = sizeof(bool);
		V_0 = ((int64_t)((int32_t)L_0));
		uint32_t L_1 = sizeof(UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2);
		V_1 = ((int64_t)((int32_t)L_1));
		V_2 = ((int64_t)4);
		int64_t L_2 = V_0;
		int32_t L_3 = ___0_length;
		int64_t L_4;
		L_4 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_2, ((int64_t)L_3))), ((int32_t)64), NULL);
		V_3 = L_4;
		int64_t L_5 = V_1;
		int32_t L_6 = ___0_length;
		int64_t L_7;
		L_7 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_5, ((int64_t)L_6))), ((int32_t)64), NULL);
		V_4 = L_7;
		int64_t L_8 = V_2;
		int32_t L_9 = ___0_length;
		int64_t L_10;
		L_10 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_8, ((int64_t)L_9))), ((int32_t)64), NULL);
		V_5 = L_10;
		int64_t L_11 = V_2;
		int64_t L_12 = ___1_bucketLength;
		int64_t L_13;
		L_13 = CollectionHelper_Align_mC40859378CEE3C9844C39FAC725566EBA5E7BD82(((int64_t)il2cpp_codegen_multiply(L_11, L_12)), ((int32_t)64), NULL);
		V_6 = L_13;
		int64_t L_14 = V_3;
		int64_t L_15 = V_4;
		int64_t L_16 = V_5;
		int64_t L_17 = V_6;
		V_7 = ((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(((int64_t)il2cpp_codegen_add(L_14, L_15)), L_16)), L_17));
		int64_t* L_18 = ___2_keyOffset;
		int64_t L_19 = V_3;
		*((int64_t*)L_18) = (int64_t)L_19;
		int64_t* L_20 = ___3_nextOffset;
		int64_t* L_21 = ___2_keyOffset;
		int64_t L_22 = *((int64_t*)L_21);
		int64_t L_23 = V_4;
		*((int64_t*)L_20) = (int64_t)((int64_t)il2cpp_codegen_add(L_22, L_23));
		int64_t* L_24 = ___4_bucketOffset;
		int64_t* L_25 = ___3_nextOffset;
		int64_t L_26 = *((int64_t*)L_25);
		int64_t L_27 = V_5;
		*((int64_t*)L_24) = (int64_t)((int64_t)il2cpp_codegen_add(L_26, L_27));
		int64_t L_28 = V_7;
		V_8 = L_28;
		goto IL_006a;
	}

IL_006a:
	{
		int64_t L_29 = V_8;
		return L_29;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m7DB5016BFFD2DCD6880EE18D7A729376915F9474_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_m49E855A91D083A54CFF44DEB13335123AD3C4001_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t2FF0F1BBD323229A2F9ABD52397EA6EF9B105E09 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mAB07175AB7AA23FBCB3571D1F770E945969E8567_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_m928DDE25B482FFC074347D0A5DB4DBF5FFCD81D7_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t0215017C25A7FD4BE98E0A8000F004E7D119560D ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m7038B50B7446591159A4FD4B5C96EC436C073DEC_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		int32_t L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mBEE29F916447CCFACE10E7F9B5C377FEAB0CC436_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t25F6CEC65DB0532CB91E2B2890FF6C2D52A210A3 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		int64_t L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(int64_t, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m1819841513C270077BA20D3C6C82D3A5F02F378E_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tD5B3D215346B1527FA237140626E9D632A0EE488 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mBC5FD6BA6693913CD1A82461A39609D9F97AC4FC_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t8E236FFFE7B598011354300BFCED0A15647BBC5D ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_m2F54096D986C61FDC9B4C5A1C796193AF5CC70F5_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tD50C1560C0B57B735333075DF206AB11B0E18565 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mB663D8448803FC18638B356861F795D575297B64_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t275C00CC374DEA66C69B3BB3992116F315A8E934 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		uint16_t L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(uint16_t, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mAAD7AAAE80C57C80CC140F956DA6FAEF8360F6B7_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		uint32_t L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(uint32_t, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisIl2CppFullySharedGenericStruct_m7137096325EC50712B1665E2B5937352A1B004F2_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_TKey_tA421440C75A581A02A933BEA4E233521D2CE2584 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 4));
	const Il2CppFullySharedGenericStruct L_13 = alloca(SizeOf_TKey_tA421440C75A581A02A933BEA4E233521D2CE2584);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = ((  int32_t (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		InvokerActionInvoker3< void*, int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)), il2cpp_rgctx_method(method->rgctx_data, 3), NULL, (void*)L_11, L_12, (Il2CppFullySharedGenericStruct*)L_13);
		InvokerActionInvoker2< int32_t, Il2CppFullySharedGenericStruct >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 5)), il2cpp_rgctx_method(method->rgctx_data, 5), (&___1_result), L_9, L_13);
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyArray_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_m6A62A986210A09AEC9B7955927D38BE37E7FEC37_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tF0A83538A02306EA7A8F22FEA945A6961365E8C8 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	bool V_6 = false;
	bool V_7 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		goto IL_0063;
	}

IL_001e:
	{
		int32_t* L_5 = V_0;
		int32_t L_6 = V_2;
		int32_t L_7 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_5, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_6), 4))))));
		V_5 = L_7;
		goto IL_0050;
	}

IL_002a:
	{
		int32_t L_8 = V_3;
		int32_t L_9 = L_8;
		V_3 = ((int32_t)il2cpp_codegen_add(L_9, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_10 = ___0_data;
		NullCheck(L_10);
		uint8_t* L_11 = L_10->___keys;
		int32_t L_12 = V_5;
		UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 L_13;
		L_13 = UnsafeUtility_ReadArrayElement_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_mC1917A7646F09213727BC23D1069EE21D2BF3920_inline((void*)L_11, L_12, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2, ((&___1_result))->___m_Buffer, L_9, (L_13));
		int32_t* L_14 = V_1;
		int32_t L_15 = V_5;
		int32_t L_16 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_15), 4))))));
		V_5 = L_16;
	}

IL_0050:
	{
		int32_t L_17 = V_5;
		V_6 = (bool)((((int32_t)((((int32_t)L_17) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_18 = V_6;
		if (L_18)
		{
			goto IL_002a;
		}
	}
	{
		int32_t L_19 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0063:
	{
		int32_t L_20 = V_2;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_21 = ___0_data;
		NullCheck(L_21);
		int32_t L_22 = L_21->___bucketCapacityMask;
		if ((((int32_t)L_20) > ((int32_t)L_22)))
		{
			goto IL_0073;
		}
	}
	{
		int32_t L_23 = V_3;
		int32_t L_24 = V_4;
		G_B8_0 = ((((int32_t)L_23) < ((int32_t)L_24))? 1 : 0);
		goto IL_0074;
	}

IL_0073:
	{
		G_B8_0 = 0;
	}

IL_0074:
	{
		V_7 = (bool)G_B8_0;
		bool L_25 = V_7;
		if (L_25)
		{
			goto IL_001e;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mEB9D0186E3BF1693EFE4B9EBAD492C9A5C75DAB6_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t67E664F1A04EFB50F19C4280E9F48C2DB97FA6AE ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m5240736CF4A860AEF91AB40A56C0FEA6010842F3((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588* L_10 = (NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588* L_16 = (NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC3C2330139E264EC4EB952292BE242F6511CA8E6_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t6F346645686D9ED3DF9980242A60F6343D9BD346 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m4BCBCB4D1AC60559B41D33E7097084B82F700097((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t2FF0F1BBD323229A2F9ABD52397EA6EF9B105E09* L_10 = (NativeArray_1_t2FF0F1BBD323229A2F9ABD52397EA6EF9B105E09*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mFF4AF9444162D21D606EE7887EACF2698D800A22_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t44905B1CEBDF9B21B174B6B004607DB160A3179F ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_mDC9713B737CE15E2D2763FB40645AF1983C27BD5((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tB7D9C79EBF3AEA35F54C11110B41E68AE0A214F6* L_16 = (NativeArray_1_tB7D9C79EBF3AEA35F54C11110B41E68AE0A214F6*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mDE41FFE3160D153E47D8D93EF29E4B9C3DB23D20_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m54F3A0DFCF72C62901702FEA7981E4C144D7A029_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t9A441098264B807CEA641B8EA0B3A6099BE560D6 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_mCE5C2D5A8F53DEC83DF1BE432C8EA289E3F41CEF((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t79AF901C035FC0E2DB95A6C857E2A6FE26138AF6* L_16 = (NativeArray_1_t79AF901C035FC0E2DB95A6C857E2A6FE26138AF6*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mD0CAE37D791EB711A170B7F9BDCEAB6ED472C3E3_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m2078926A64CB9AAB90B7EF256958C508104A9D97_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t5819D7BFEF9DAB21CE5F03E645D249BAF749DD29 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_mB7698B5AC6930259C71BDD34DE4AF096F07CED38((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t712BAFB3B7FD80607F953479B6E3EE2D54BD9446* L_16 = (NativeArray_1_t712BAFB3B7FD80607F953479B6E3EE2D54BD9446*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_mBA0DB77712EA2A549142CA67C7AE16B595A60205_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mA03F53939352AD52E121E78DAABC5895D0CFF08D_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tA2DA312B446D1C4E01DFB5BABCD5FD907A6D98BB ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m686910AE937E76C8D8BAC2FA971F98F55668D1A1((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t727E0B11B40EA7D6477F67D79DA7B7F7C383735E* L_16 = (NativeArray_1_t727E0B11B40EA7D6477F67D79DA7B7F7C383735E*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF42F014B5AA9C633D3FCAB3FFF26646E54395BD8_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m25B1FF2DD44BBEC084000393CE299617A0AC1E75_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t69F48B55F7CB9E53D8DA8AC22D78A9FE2C4ACCBB ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_mFCC843E8A6076977EDC711B85D5A812409F96A92((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t8E042B4249B3126F27EE49887D2507798DC25F2C* L_16 = (NativeArray_1_t8E042B4249B3126F27EE49887D2507798DC25F2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m86FC76E325E9D3B56133BB7A193BAF12B47FBCC1_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m3DB176E612D3F9E0446BCC0B1803F073E38F8254_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tCBCD37F108CC29AF7DD444E7DADEBD5DA915330C ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m8AD3EFA2ADD479EB790D599C9BF297BBFE6A6E22((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mE23C8F88E36EF9329976E2C6CC35A6D3BD865145_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tF6C01F5E2A268EC719FE153919FBB3C364489509 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m231B05B8DA777B05DDB21032416A0EFAAA7770D1((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62* L_10 = (NativeArray_1_t3C666A50D3E0F5803B63036EC771A974D48FFF62*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tFBC4A05617FFC862FFD574140F343DA82BF818B2* L_16 = (NativeArray_1_tFBC4A05617FFC862FFD574140F343DA82BF818B2*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m6992824D0CDD449EFB81329A07FAC0AC09F03218_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mD2C0658BEFB72A5989DECCFB14A03DCF74B71E46_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t26BFA27003E5D25D65B6A5407F9D5F228E8740A6 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m7409414C2CB2F8B001EDB340C888A267B2668251((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t0215017C25A7FD4BE98E0A8000F004E7D119560D* L_10 = (NativeArray_1_t0215017C25A7FD4BE98E0A8000F004E7D119560D*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t1864D78BA2DD264CF10C4F534C4A53B8D45EDFC3* L_16 = (NativeArray_1_t1864D78BA2DD264CF10C4F534C4A53B8D45EDFC3*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m2ADCB1A0DE89B2E013B4BB7D300F81AECACFE8EA_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFE9C57688981A69BB82626EC5B584E4C570A0B1A_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tF51A55D75DBB6D766D8D5B99DC29C3762BA12496 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m0E87F558427CED97F501D4AB4A5F0A78A2B39723((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_10 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		int32_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m1E750A8A323FD50BB2502C9516B48DFD6E8EA959_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t00C5D2C7AF9D9E3622CAB210E6E11E33C4881C8A ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m02951443FBCF5B78507CC0C916DA7B406A6E2FFC((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_10 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		int32_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t497DD754C21E03BAC4C1F2BB7A3BB0025FF4EB88* L_16 = (NativeArray_1_t497DD754C21E03BAC4C1F2BB7A3BB0025FF4EB88*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m8460969BD9998B0DFF865186874B2414FEED12C8_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mB322962F8C7C2CBDA957DADE5465A9DD402B48B4_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t6E475CFF982A098C6F4413A38B259B41803D683C ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m9C73B4588FF12C4843A4CA6E3548C69471837599((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t25F6CEC65DB0532CB91E2B2890FF6C2D52A210A3* L_10 = (NativeArray_1_t25F6CEC65DB0532CB91E2B2890FF6C2D52A210A3*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		int64_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(int64_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tD66836534F00AADA4D14B93A8662AF8DA2D65075* L_16 = (NativeArray_1_tD66836534F00AADA4D14B93A8662AF8DA2D65075*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m14BBB3C75525EE04E460C0A3D179C85C5BC7C612_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m422775D04D012066A019DDC404A8E20ED1A6B69B_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tCBEADB7142CDB924EF564F51C98AFFA7C6A162BC ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_mB5AA196744005B710F1B67E470ADA3C78AAA8E84((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_tD5B3D215346B1527FA237140626E9D632A0EE488* L_10 = (NativeArray_1_tD5B3D215346B1527FA237140626E9D632A0EE488*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588* L_16 = (NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4E1351262AA9FAA749ADAC271C6A88355D3D4D85_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t5B99C2646C28A5C04DF011E6E993BDE26A27A953 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m354A6BD5F31BC54C9F3D2B8A51EF1C71C9005648((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t8E236FFFE7B598011354300BFCED0A15647BBC5D* L_10 = (NativeArray_1_t8E236FFFE7B598011354300BFCED0A15647BBC5D*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m0FD83819EAA8814C2983899C25F8FC039FA6A425_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t2FD6C3D7785A9C84CF994FC08628D01B7947C7C1 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m9FF445C8935DB92A9DB6A61D0428F6E1301EA17A((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_tD50C1560C0B57B735333075DF206AB11B0E18565* L_10 = (NativeArray_1_tD50C1560C0B57B735333075DF206AB11B0E18565*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB81F6284A26CFB44DCBD459073C5814B76618249_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t31F3CE04B96C7A9192BEB52001190AF4216A7990 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m5D960A4C192B677E942F8EE6033822A69C5F22F8((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t275C00CC374DEA66C69B3BB3992116F315A8E934* L_10 = (NativeArray_1_t275C00CC374DEA66C69B3BB3992116F315A8E934*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		uint16_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(uint16_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m2BA4D4A5293E287CF637DB970992593592D80C74_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t699CBC5BC6A801E2B73A1521AF599E34499857B8 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m08640C8B3F96CF59CE7C409ED1E5A53510CE219F((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184* L_10 = (NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		uint32_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(uint32_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t60C2883F9133642CF381234B3DD254605DD4F0A5* L_16 = (NativeArray_1_t60C2883F9133642CF381234B3DD254605DD4F0A5*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m80C61348EC8F9D4F73F0AAA4A69A028E6D599DCD_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m07B41A0BA900140C60A8DA63F2CFA357FC3FFA83_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tD4417ED5E6C2DAC8437C9159D79F622F83C72D39 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m0DD220DB30C031B6200A4119D0DCF56B565846E6((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184* L_10 = (NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		uint32_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(uint32_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t703E60DA65EBF076EB4916D3B9D9987A5A09B9CC* L_16 = (NativeArray_1_t703E60DA65EBF076EB4916D3B9D9987A5A09B9CC*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m2A66DBE83F9498AC60B42C9A0B1C7E4401B4CDCC_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m07F57872193FA2EAB888B9D0FAECEC7F5C78B997_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t4AA59D24D1616F48DCDB5CB6C4ABA336C51B1764 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m8C99316414D47C54EF1AFF05D069266C1FE65840((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184* L_10 = (NativeArray_1_t453E3DEA4CC9F1056F24E417C3308C35174BC184*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		uint32_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(uint32_t, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C* L_16 = (NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		int32_t L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericStruct_mE7B20EAFC97698B67540C38AF16BB6595C8B786D_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_t97DEBC72840B73CFCEE6195C4C495C675E96FAF1 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_TKey_t144C42ACB2DDADF052103F43EA00E1E62D94C90F = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 5));
	const Il2CppFullySharedGenericStruct L_15 = alloca(SizeOf_TKey_t144C42ACB2DDADF052103F43EA00E1E62D94C90F);
	const uint32_t SizeOf_TValue_t4FEA332ED977D95FEBFE603FCE1F5250E543CBBF = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 10));
	const Il2CppFullySharedGenericStruct L_21 = alloca(SizeOf_TValue_t4FEA332ED977D95FEBFE603FCE1F5250E543CBBF);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = ((  int32_t (*) (NativeKeyValueArrays_2_t97DEBC72840B73CFCEE6195C4C495C675E96FAF1*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_10 = (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		InvokerActionInvoker3< void*, int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)), il2cpp_rgctx_method(method->rgctx_data, 4), NULL, (void*)L_13, L_14, (Il2CppFullySharedGenericStruct*)L_15);
		InvokerActionInvoker2< int32_t, Il2CppFullySharedGenericStruct >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 6)), il2cpp_rgctx_method(method->rgctx_data, 6), L_10, L_11, L_15);
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18* L_16 = (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		InvokerActionInvoker3< void*, int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 9)), il2cpp_rgctx_method(method->rgctx_data, 9), NULL, (void*)L_19, L_20, (Il2CppFullySharedGenericStruct*)L_21);
		InvokerActionInvoker2< int32_t, Il2CppFullySharedGenericStruct >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 11)), il2cpp_rgctx_method(method->rgctx_data, 11), L_16, L_17, L_21);
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetKeyValueArrays_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m1332BA571DEA39C153344466C70A60A10325F496_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeKeyValueArrays_2_tA9D90BDB6A6A5B59E496BEBBDA8082D2A0598A49 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = NativeKeyValueArrays_2_get_Length_m4A745207D9267220A0F145A1E29332EF7D212017((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_008b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0078;
	}

IL_0032:
	{
		NativeArray_1_tF0A83538A02306EA7A8F22FEA945A6961365E8C8* L_10 = (NativeArray_1_tF0A83538A02306EA7A8F22FEA945A6961365E8C8*)(&(&___1_result)->___Keys);
		int32_t L_11 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___keys;
		int32_t L_14 = V_6;
		UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_mC1917A7646F09213727BC23D1069EE21D2BF3920_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 4));
		IL2CPP_NATIVEARRAY_SET_ITEM(UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2, (L_10)->___m_Buffer, L_11, (L_15));
		NativeArray_1_t107C57D0357BCF9956A60495CD8FAADDF1D26AFB* L_16 = (NativeArray_1_t107C57D0357BCF9956A60495CD8FAADDF1D26AFB*)(&(&___1_result)->___Values);
		int32_t L_17 = V_3;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_18 = ___0_data;
		NullCheck(L_18);
		uint8_t* L_19 = L_18->___values;
		int32_t L_20 = V_6;
		bool L_21;
		L_21 = UnsafeUtility_ReadArrayElement_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mF765CB2E5FD631DFB79C25DE656F3C1BB359B995_inline((void*)L_19, L_20, il2cpp_rgctx_method(method->rgctx_data, 9));
		IL2CPP_NATIVEARRAY_SET_ITEM(bool, (L_16)->___m_Buffer, L_17, (L_21));
		int32_t L_22 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_22, 1));
		int32_t* L_23 = V_1;
		int32_t L_24 = V_6;
		int32_t L_25 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_23, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_24), 4))))));
		V_6 = L_25;
	}

IL_0078:
	{
		int32_t L_26 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_26) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_27 = V_7;
		if (L_27)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_28 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_28, 1));
	}

IL_008b:
	{
		int32_t L_29 = V_2;
		int32_t L_30 = V_5;
		if ((((int32_t)L_29) > ((int32_t)L_30)))
		{
			goto IL_0097;
		}
	}
	{
		int32_t L_31 = V_3;
		int32_t L_32 = V_4;
		G_B8_0 = ((((int32_t)L_31) < ((int32_t)L_32))? 1 : 0);
		goto IL_0098;
	}

IL_0097:
	{
		G_B8_0 = 0;
	}

IL_0098:
	{
		V_8 = (bool)G_B8_0;
		bool L_33 = V_8;
		if (L_33)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mAFA081C9B2AD7C9734C843DA3296E66A06F64258_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t60C2883F9133642CF381234B3DD254605DD4F0A5 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m80C61348EC8F9D4F73F0AAA4A69A028E6D599DCD_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mABDA427688BDD14DFA11637AD6696A2F5D0CEBE9_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tB7D9C79EBF3AEA35F54C11110B41E68AE0A214F6 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mDE41FFE3160D153E47D8D93EF29E4B9C3DB23D20_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m220EEBA1CA1041FDE72E8FF11DF2DA3BB751B890_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t79AF901C035FC0E2DB95A6C857E2A6FE26138AF6 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mD0CAE37D791EB711A170B7F9BDCEAB6ED472C3E3_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mC207F978B303B5F48108C11804D921E6F240342D_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t107C57D0357BCF9956A60495CD8FAADDF1D26AFB ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		bool L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mF765CB2E5FD631DFB79C25DE656F3C1BB359B995_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(bool, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m834956570BC1506CF67FD2F4F2068BE24047B6C6_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t512F97AF64D482FD56DDCB57802B21523920E588 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mDBDE437C405F193BED2D388250926BE5A75DABEE_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t1864D78BA2DD264CF10C4F534C4A53B8D45EDFC3 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m2ADCB1A0DE89B2E013B4BB7D300F81AECACFE8EA_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m6B65196A2788ABA4AED3615CEE76C9F7064B9061_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t712BAFB3B7FD80607F953479B6E3EE2D54BD9446 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_mBA0DB77712EA2A549142CA67C7AE16B595A60205_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_m0E8CE5C728489D98BC6F0B92382E2326A0E0769D_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t727E0B11B40EA7D6477F67D79DA7B7F7C383735E ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF42F014B5AA9C633D3FCAB3FFF26646E54395BD8_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m200DA9B2BC8B0CEB99FD9F46ED953E12251AC2B4_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t703E60DA65EBF076EB4916D3B9D9987A5A09B9CC ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m2A66DBE83F9498AC60B42C9A0B1C7E4401B4CDCC_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m9E1C36193953F8B34B0DAA5D4C5EE5B57D78B5AD_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t8E042B4249B3126F27EE49887D2507798DC25F2C ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m86FC76E325E9D3B56133BB7A193BAF12B47FBCC1_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4769F528CC3AB04D768F83B2A2A016AE29C5F87E_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tA833EB7E3E1C9AF82C37976AD964B8D4BAC38B2C ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		int32_t L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(int32_t, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisIl2CppFullySharedGenericStruct_m127A626CD960E02AEBF7B88B7A8F3A9401445A69_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_TValue_t0232C4E00D4E39FC209358F87310CF1B96E0619E = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 4));
	const Il2CppFullySharedGenericStruct L_15 = alloca(SizeOf_TValue_t0232C4E00D4E39FC209358F87310CF1B96E0619E);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = ((  int32_t (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___1_result), il2cpp_rgctx_method(method->rgctx_data, 1));
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		InvokerActionInvoker3< void*, int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)), il2cpp_rgctx_method(method->rgctx_data, 3), NULL, (void*)L_13, L_14, (Il2CppFullySharedGenericStruct*)L_15);
		InvokerActionInvoker2< int32_t, Il2CppFullySharedGenericStruct >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 5)), il2cpp_rgctx_method(method->rgctx_data, 5), (&___1_result), L_11, L_15);
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m446BDDF680B38A45E3F4718BE1B6A5A518D253AC_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tFBC4A05617FFC862FFD574140F343DA82BF818B2 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m6992824D0CDD449EFB81329A07FAC0AC09F03218_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_mF1597D81CF060DA43380933C7653AEE5D655FE2F_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_t497DD754C21E03BAC4C1F2BB7A3BB0025FF4EB88 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m8460969BD9998B0DFF865186874B2414FEED12C8_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_GetValueArray_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mC7A8EDE89E178E8AA0E2DD67B7467597DC8026CE_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, NativeArray_1_tD66836534F00AADA4D14B93A8662AF8DA2D65075 ___1_result, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	int32_t* V_0 = NULL;
	int32_t* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	bool V_7 = false;
	bool V_8 = false;
	int32_t G_B8_0 = 0;
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_0 = ___0_data;
		NullCheck(L_0);
		uint8_t* L_1 = L_0->___buckets;
		V_0 = (int32_t*)L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		uint8_t* L_3 = L_2->___next;
		V_1 = (int32_t*)L_3;
		V_2 = 0;
		V_3 = 0;
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___1_result))->___m_Length);
		V_4 = L_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		V_5 = L_6;
		goto IL_006b;
	}

IL_0026:
	{
		int32_t* L_7 = V_0;
		int32_t L_8 = V_2;
		int32_t L_9 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_7, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_8), 4))))));
		V_6 = L_9;
		goto IL_0058;
	}

IL_0032:
	{
		int32_t L_10 = V_3;
		int32_t L_11 = L_10;
		V_3 = ((int32_t)il2cpp_codegen_add(L_11, 1));
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_12 = ___0_data;
		NullCheck(L_12);
		uint8_t* L_13 = L_12->___values;
		int32_t L_14 = V_6;
		ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 L_15;
		L_15 = UnsafeUtility_ReadArrayElement_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m14BBB3C75525EE04E460C0A3D179C85C5BC7C612_inline((void*)L_13, L_14, il2cpp_rgctx_method(method->rgctx_data, 3));
		IL2CPP_NATIVEARRAY_SET_ITEM(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899, ((&___1_result))->___m_Buffer, L_11, (L_15));
		int32_t* L_16 = V_1;
		int32_t L_17 = V_6;
		int32_t L_18 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_16, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_17), 4))))));
		V_6 = L_18;
	}

IL_0058:
	{
		int32_t L_19 = V_6;
		V_7 = (bool)((((int32_t)((((int32_t)L_19) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_20 = V_7;
		if (L_20)
		{
			goto IL_0032;
		}
	}
	{
		int32_t L_21 = V_2;
		V_2 = ((int32_t)il2cpp_codegen_add(L_21, 1));
	}

IL_006b:
	{
		int32_t L_22 = V_2;
		int32_t L_23 = V_5;
		if ((((int32_t)L_22) > ((int32_t)L_23)))
		{
			goto IL_0077;
		}
	}
	{
		int32_t L_24 = V_3;
		int32_t L_25 = V_4;
		G_B8_0 = ((((int32_t)L_24) < ((int32_t)L_25))? 1 : 0);
		goto IL_0078;
	}

IL_0077:
	{
		G_B8_0 = 0;
	}

IL_0078:
	{
		V_8 = (bool)G_B8_0;
		bool L_26 = V_8;
		if (L_26)
		{
			goto IL_0026;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m2F7D855BF39ABD687C33BD95094A05C80B0CE4C5_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mCBA4B05BA324F08F46C24FAEAF26B6FA71706FDC(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		il2cpp_codegen_runtime_class_init_inline(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_il2cpp_TypeInfo_var);
		int32_t L_68;
		L_68 = ConnectionId_GetHashCode_m7CD005F8169BE20415267EA691CCBEAC252DE0F2((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m3998D4C65643516D842CF6D61C4513767A18FA63_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEA2D219466CA70D0772888D0B2AA5BDF95B3D7B0(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = DrawKey_GetHashCode_m6E3C5F3D42D02D8035AA7E7B5501FDBC1551F4E5((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mED6152AEEA0EBDE058047C489091A8AB2C5FC6AB_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_m618DEF61FF37029CC5BB378A9DF74D17BF431F06(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mC8259C1CD55EE80A391FFE1F3824646EC3E499CD_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_m1FF8E4DB8A087F7A862C15EEC0913C3FCD81FFF5(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m8E3130F44C4F536E5688D41A5E0D72968FD87D5D_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_m4F68FC2BD5F10999A62EE62207C4B8B4E57489D1(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF394020DE5FEF55FD41D472FCFEDFB94EC21E18C_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mBBBCD7A3795516F652E2A209921D9F365FEF117D(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m60D8A6A4D2FC5E25CD0C3229AD6E6B568C384C32_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_mFCFCB281237C3FD181B82326F1CDC74ECDC26835(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m8CA3249E613156F5440F57EDC2841CC44B9CF479_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m28DB190FC25AC6811E2F609C118B25148772E8ED(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m2364DC56ECD880B5DA95E22416B6DF6D885C4246_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_mB677466F5AF7999905E08C12DA5F85B135C65DA2(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m112A1240FB7BFA922A7EE2BB1418D7C896252A88_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_mA6587978CB654E5F829A8E1413F71B7210CE52FB(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = FixedString128Bytes_GetHashCode_mB211F7E224953364EE91770921BA59760A0E4428((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m42588E02F8FB973684E7752BF762ACBAF0AF200C_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	int32_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB151D38E73EEDFB7B82F1F6498C9906EB81D402A(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		int32_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = Int32_GetHashCode_m253D60FF7527A483E91004B7A2366F13E225E295((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_mED51823316D888F4C0BBD35347050157D421DF94_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	int32_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m7CF1071B4128F92A069498179CE85F80CB5AEBE1(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		int32_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = Int32_GetHashCode_m253D60FF7527A483E91004B7A2366F13E225E295((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mE6059242AA9F8B3F4FFE61DC417897FDE9276AE8_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	int64_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_mDDA0397F7F044E6209E9F8A2D66077E9E5CB485E(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(int64_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		int64_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = Int64_GetHashCode_mDB050BE2AC244D92B14D1DF725AAD279CDC48496((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m48ACCC1161D7068E2D78AD2DB841F4F4CBBA3BB5_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_m8BB02CE71160A2514C280DBDC8A5253F93E3689E(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = NetworkEndpoint_GetHashCode_mA1753E7F3CD5AC227330A2A63F6623BA737A7696((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m3DEC8FE0C45757F13739FD01205D6A4138E8E9CD_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m47E7EA1FA34E44C4B7E215FA109EFC5813E93948(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = RangeKey_GetHashCode_mC38845B6A1CC657D6F6B1149E7448AA6A3EF3257((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m441029D76B1BEA20FAF297CD91EC7DCA44CC2142_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 V_19;
	memset((&V_19), 0, sizeof(V_19));
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mE5DE2AA98BC9864F8C516BE7C30A490B373C35D7(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		il2cpp_codegen_runtime_class_init_inline(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_il2cpp_TypeInfo_var);
		int32_t L_68;
		L_68 = SharedInstanceHandle_GetHashCode_m5B97E179A78BD59969291F66E286E00873FC120D((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1D97D21F2B7CEF8D0074591D0D929BC2F005D697_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	uint16_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m20514B296368E3BD19C54B8257A916FCC7837FCB(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(uint16_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		uint16_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = UInt16_GetHashCode_m534E5103D0DA9C6FCED4F2F007993D3E38165200((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m34D174EB98DF528CE45E4B40C92E22D9415A88A7_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	uint32_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_mF58A503E41D28232BA45EF49B867720056B3C3EB(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(uint32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		uint32_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = UInt32_GetHashCode_mB9A03A037C079ADF0E61516BECA1AB05F92266BC((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_mBD7A8412C7ED558BD4891BE34D5D811409097B84_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	uint32_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m84BBCADD738D8E17724D100724F1173F24CC9D46(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(uint32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		uint32_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = UInt32_GetHashCode_mB9A03A037C079ADF0E61516BECA1AB05F92266BC((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mED337C060A7869E7A836D5F3C6671B9500396F53_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	uint32_t V_19 = 0;
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = UnsafeParallelHashMapData_CalculateDataSize_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m92740FC50E0692378ABED6D572FE0AA5FFF18DFB(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = sizeof(int32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = sizeof(uint32_t);
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		uint32_t L_67;
		L_67 = UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_inline((void*)L_65, L_66, il2cpp_rgctx_method(method->rgctx_data, 3));
		V_19 = L_67;
		int32_t L_68;
		L_68 = UInt32_GetHashCode_mB9A03A037C079ADF0E61516BECA1AB05F92266BC((&V_19), il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_69 = V_8;
		V_18 = ((int32_t)(L_68&L_69));
		int32_t* L_70 = V_16;
		int32_t L_71 = V_17;
		uint8_t* L_72 = V_7;
		int32_t L_73 = V_18;
		int32_t L_74 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_72, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_73), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_70, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_71), 4))))) = (int32_t)L_74;
		uint8_t* L_75 = V_7;
		int32_t L_76 = V_18;
		int32_t L_77 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_75, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_76), 4))))) = (int32_t)L_77;
	}

IL_0172:
	{
		int32_t* L_78 = V_15;
		int32_t L_79 = V_14;
		int32_t L_80 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_78, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_79), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_80) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_81 = V_20;
		if (L_81)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_82 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_82, 1));
	}

IL_018e:
	{
		int32_t L_83 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_84 = ___0_data;
		NullCheck(L_84);
		int32_t L_85 = L_84->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_83) > ((int32_t)L_85))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_86 = V_21;
		if (L_86)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_87 = ___0_data;
		NullCheck(L_87);
		uint8_t* L_88 = L_87->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_89 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_88, L_89, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_90 = ___0_data;
		NullCheck(L_90);
		int32_t L_91 = L_90->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_92 = ___0_data;
		NullCheck(L_92);
		int32_t L_93 = L_92->___keyCapacity;
		V_22 = (bool)((((int32_t)L_91) > ((int32_t)L_93))? 1 : 0);
		bool L_94 = V_22;
		if (!L_94)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_95 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		NullCheck(L_96);
		int32_t L_97 = L_96->___keyCapacity;
		NullCheck(L_95);
		L_95->___allocatedIndexLength = L_97;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_98 = ___0_data;
		uint8_t* L_99 = V_4;
		NullCheck(L_98);
		L_98->___values = L_99;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_100 = ___0_data;
		uint8_t* L_101 = V_5;
		NullCheck(L_100);
		L_100->___keys = L_101;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_102 = ___0_data;
		uint8_t* L_103 = V_6;
		NullCheck(L_102);
		L_102->___next = L_103;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_104 = ___0_data;
		uint8_t* L_105 = V_7;
		NullCheck(L_104);
		L_104->___buckets = L_105;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_106 = ___0_data;
		int32_t L_107 = ___1_newCapacity;
		NullCheck(L_106);
		L_106->___keyCapacity = L_107;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_108 = ___0_data;
		int32_t L_109 = V_8;
		NullCheck(L_108);
		L_108->___bucketCapacityMask = L_109;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnsafeParallelHashMapData_ReallocateHashMap_TisIl2CppFullySharedGenericStruct_TisIl2CppFullySharedGenericStruct_mD75A84E690BE573D5B2F40EF59DFD1AC4703BFED_gshared (UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* ___0_data, int32_t ___1_newCapacity, int64_t ___2_newBucketCapacity, AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___3_label, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_TKey_t5C6CF5C92FA36F580A298FBAD5C0C26409B948E2 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const uint32_t SizeOf_TValue_tC69328621FD17954468B8EF13B10C5DE9B45FA92 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	void* L_68 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data(method->rgctx_data, 2)));
	const Il2CppFullySharedGenericStruct L_67 = alloca(SizeOf_TKey_t5C6CF5C92FA36F580A298FBAD5C0C26409B948E2);
	int64_t V_0 = 0;
	int64_t V_1 = 0;
	int64_t V_2 = 0;
	int64_t V_3 = 0;
	uint8_t* V_4 = NULL;
	uint8_t* V_5 = NULL;
	uint8_t* V_6 = NULL;
	uint8_t* V_7 = NULL;
	int32_t V_8 = 0;
	bool V_9 = false;
	int32_t V_10 = 0;
	bool V_11 = false;
	int32_t V_12 = 0;
	bool V_13 = false;
	int32_t V_14 = 0;
	int32_t* V_15 = NULL;
	int32_t* V_16 = NULL;
	int32_t V_17 = 0;
	int32_t V_18 = 0;
	Il2CppFullySharedGenericStruct V_19 = alloca(SizeOf_TKey_t5C6CF5C92FA36F580A298FBAD5C0C26409B948E2);
	memset(V_19, 0, SizeOf_TKey_t5C6CF5C92FA36F580A298FBAD5C0C26409B948E2);
	bool V_20 = false;
	bool V_21 = false;
	bool V_22 = false;
	int32_t G_B3_0 = 0;
	{
		int64_t L_0 = ___2_newBucketCapacity;
		int64_t L_1;
		L_1 = math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline(L_0, NULL);
		___2_newBucketCapacity = L_1;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_2 = ___0_data;
		NullCheck(L_2);
		int32_t L_3 = L_2->___keyCapacity;
		int32_t L_4 = ___1_newCapacity;
		if ((!(((uint32_t)L_3) == ((uint32_t)L_4))))
		{
			goto IL_0020;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_5 = ___0_data;
		NullCheck(L_5);
		int32_t L_6 = L_5->___bucketCapacityMask;
		int64_t L_7 = ___2_newBucketCapacity;
		G_B3_0 = ((((int64_t)((int64_t)((int32_t)il2cpp_codegen_add(L_6, 1)))) == ((int64_t)L_7))? 1 : 0);
		goto IL_0021;
	}

IL_0020:
	{
		G_B3_0 = 0;
	}

IL_0021:
	{
		V_9 = (bool)G_B3_0;
		bool L_8 = V_9;
		if (!L_8)
		{
			goto IL_002d;
		}
	}
	{
		goto IL_0202;
	}

IL_002d:
	{
		int32_t L_9 = ___1_newCapacity;
		int64_t L_10 = ___2_newBucketCapacity;
		int64_t L_11;
		L_11 = ((  int64_t (*) (int32_t, int64_t, int64_t*, int64_t*, int64_t*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_9, L_10, (&V_0), (&V_1), (&V_2), il2cpp_rgctx_method(method->rgctx_data, 0));
		V_3 = L_11;
		int64_t L_12 = V_3;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_13 = ___3_label;
		void* L_14;
		L_14 = Unmanaged_Allocate_m7310B1FE896DEFFA18303D961C9859C8FF3D21E5(L_12, ((int32_t)64), L_13, NULL);
		V_4 = (uint8_t*)L_14;
		uint8_t* L_15 = V_4;
		int64_t L_16 = V_0;
		V_5 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_15, ((intptr_t)L_16)));
		uint8_t* L_17 = V_4;
		int64_t L_18 = V_1;
		V_6 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_17, ((intptr_t)L_18)));
		uint8_t* L_19 = V_4;
		int64_t L_20 = V_2;
		V_7 = ((uint8_t*)il2cpp_codegen_add((intptr_t)L_19, ((intptr_t)L_20)));
		uint8_t* L_21 = V_4;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_22 = ___0_data;
		NullCheck(L_22);
		uint8_t* L_23 = L_22->___values;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_24 = ___0_data;
		NullCheck(L_24);
		int32_t L_25 = L_24->___keyCapacity;
		uint32_t L_26 = SizeOf_TValue_tC69328621FD17954468B8EF13B10C5DE9B45FA92;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_21, (void*)L_23, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_25), ((int64_t)((int32_t)L_26)))), NULL);
		uint8_t* L_27 = V_5;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_28 = ___0_data;
		NullCheck(L_28);
		uint8_t* L_29 = L_28->___keys;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_30 = ___0_data;
		NullCheck(L_30);
		int32_t L_31 = L_30->___keyCapacity;
		uint32_t L_32 = SizeOf_TKey_t5C6CF5C92FA36F580A298FBAD5C0C26409B948E2;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_27, (void*)L_29, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_31), ((int64_t)((int32_t)L_32)))), NULL);
		uint8_t* L_33 = V_6;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_34 = ___0_data;
		NullCheck(L_34);
		uint8_t* L_35 = L_34->___next;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_36 = ___0_data;
		NullCheck(L_36);
		int32_t L_37 = L_36->___keyCapacity;
		UnsafeUtility_MemCpy_m5CEA91ACDADC522E584AE3A2AB2B0B74393A9177((void*)L_33, (void*)L_35, ((int64_t)il2cpp_codegen_multiply(((int64_t)L_37), ((int64_t)4))), NULL);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_38 = ___0_data;
		NullCheck(L_38);
		int32_t L_39 = L_38->___keyCapacity;
		V_10 = L_39;
		goto IL_00c9;
	}

IL_00b7:
	{
		uint8_t* L_40 = V_6;
		int32_t L_41 = V_10;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_40, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_41), 4))))) = (int32_t)(-1);
		int32_t L_42 = V_10;
		V_10 = ((int32_t)il2cpp_codegen_add(L_42, 1));
	}

IL_00c9:
	{
		int32_t L_43 = V_10;
		int32_t L_44 = ___1_newCapacity;
		V_11 = (bool)((((int32_t)L_43) < ((int32_t)L_44))? 1 : 0);
		bool L_45 = V_11;
		if (L_45)
		{
			goto IL_00b7;
		}
	}
	{
		V_12 = 0;
		goto IL_00eb;
	}

IL_00d9:
	{
		uint8_t* L_46 = V_7;
		int32_t L_47 = V_12;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_46, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_47), 4))))) = (int32_t)(-1);
		int32_t L_48 = V_12;
		V_12 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_00eb:
	{
		int32_t L_49 = V_12;
		int64_t L_50 = ___2_newBucketCapacity;
		V_13 = (bool)((((int64_t)((int64_t)L_49)) < ((int64_t)L_50))? 1 : 0);
		bool L_51 = V_13;
		if (L_51)
		{
			goto IL_00d9;
		}
	}
	{
		int64_t L_52 = ___2_newBucketCapacity;
		V_8 = ((int32_t)((int64_t)il2cpp_codegen_subtract(L_52, ((int64_t)1))));
		V_14 = 0;
		goto IL_018e;
	}

IL_0106:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_53 = ___0_data;
		NullCheck(L_53);
		uint8_t* L_54 = L_53->___buckets;
		V_15 = (int32_t*)L_54;
		uint8_t* L_55 = V_6;
		V_16 = (int32_t*)L_55;
		goto IL_0172;
	}

IL_0115:
	{
		int32_t* L_56 = V_15;
		int32_t L_57 = V_14;
		int32_t L_58 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_56, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_57), 4))))));
		V_17 = L_58;
		int32_t* L_59 = V_15;
		int32_t L_60 = V_14;
		int32_t* L_61 = V_16;
		int32_t L_62 = V_17;
		int32_t L_63 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_61, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_62), 4))))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_59, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_60), 4))))) = (int32_t)L_63;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_64 = ___0_data;
		NullCheck(L_64);
		uint8_t* L_65 = L_64->___keys;
		int32_t L_66 = V_17;
		InvokerActionInvoker3< void*, int32_t, Il2CppFullySharedGenericStruct* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)), il2cpp_rgctx_method(method->rgctx_data, 3), NULL, (void*)L_65, L_66, (Il2CppFullySharedGenericStruct*)L_67);
		il2cpp_codegen_memcpy(V_19, L_67, SizeOf_TKey_t5C6CF5C92FA36F580A298FBAD5C0C26409B948E2);
		int32_t L_69;
		L_69 = ConstrainedFuncInvoker0< int32_t >::Invoke(il2cpp_rgctx_data(method->rgctx_data, 2), il2cpp_rgctx_method(method->rgctx_data, 4), L_68, (void*)(Il2CppFullySharedGenericStruct*)V_19);
		int32_t L_70 = V_8;
		V_18 = ((int32_t)(L_69&L_70));
		int32_t* L_71 = V_16;
		int32_t L_72 = V_17;
		uint8_t* L_73 = V_7;
		int32_t L_74 = V_18;
		int32_t L_75 = *((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_73, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_74), 4)))));
		*((int32_t*)((int32_t*)il2cpp_codegen_add((intptr_t)L_71, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_72), 4))))) = (int32_t)L_75;
		uint8_t* L_76 = V_7;
		int32_t L_77 = V_18;
		int32_t L_78 = V_17;
		*((int32_t*)((uint8_t*)il2cpp_codegen_add((intptr_t)L_76, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_77), 4))))) = (int32_t)L_78;
	}

IL_0172:
	{
		int32_t* L_79 = V_15;
		int32_t L_80 = V_14;
		int32_t L_81 = (*(((int32_t*)il2cpp_codegen_add((intptr_t)L_79, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)L_80), 4))))));
		V_20 = (bool)((((int32_t)((((int32_t)L_81) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_82 = V_20;
		if (L_82)
		{
			goto IL_0115;
		}
	}
	{
		int32_t L_83 = V_14;
		V_14 = ((int32_t)il2cpp_codegen_add(L_83, 1));
	}

IL_018e:
	{
		int32_t L_84 = V_14;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_85 = ___0_data;
		NullCheck(L_85);
		int32_t L_86 = L_85->___bucketCapacityMask;
		V_21 = (bool)((((int32_t)((((int32_t)L_84) > ((int32_t)L_86))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_87 = V_21;
		if (L_87)
		{
			goto IL_0106;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_88 = ___0_data;
		NullCheck(L_88);
		uint8_t* L_89 = L_88->___values;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_90 = ___3_label;
		Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271(L_89, L_90, Unmanaged_Free_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m1C432B8FDFC847D68ECC57BF4C911BA784284271_RuntimeMethod_var);
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_91 = ___0_data;
		NullCheck(L_91);
		int32_t L_92 = L_91->___allocatedIndexLength;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_93 = ___0_data;
		NullCheck(L_93);
		int32_t L_94 = L_93->___keyCapacity;
		V_22 = (bool)((((int32_t)L_92) > ((int32_t)L_94))? 1 : 0);
		bool L_95 = V_22;
		if (!L_95)
		{
			goto IL_01d3;
		}
	}
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_96 = ___0_data;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_97 = ___0_data;
		NullCheck(L_97);
		int32_t L_98 = L_97->___keyCapacity;
		NullCheck(L_96);
		L_96->___allocatedIndexLength = L_98;
	}

IL_01d3:
	{
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_99 = ___0_data;
		uint8_t* L_100 = V_4;
		NullCheck(L_99);
		L_99->___values = L_100;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_101 = ___0_data;
		uint8_t* L_102 = V_5;
		NullCheck(L_101);
		L_101->___keys = L_102;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_103 = ___0_data;
		uint8_t* L_104 = V_6;
		NullCheck(L_103);
		L_103->___next = L_104;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_105 = ___0_data;
		uint8_t* L_106 = V_7;
		NullCheck(L_105);
		L_105->___buckets = L_106;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_107 = ___0_data;
		int32_t L_108 = ___1_newCapacity;
		NullCheck(L_107);
		L_107->___keyCapacity = L_108;
		UnsafeParallelHashMapData_t43CAB3170FBB624A9CCB6F30C0EC1BB820D57926* L_109 = ___0_data;
		int32_t L_110 = V_8;
		NullCheck(L_109);
		L_109->___bucketCapacityMask = L_110;
	}

IL_0202:
	{
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int64_t math_ceilpow2_m7941384EAF6F776691CD2601130055362B0C967C_inline (int64_t ___0_x, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	{
		int64_t L_0 = ___0_x;
		___0_x = ((int64_t)il2cpp_codegen_subtract(L_0, ((int64_t)1)));
		int64_t L_1 = ___0_x;
		int64_t L_2 = ___0_x;
		___0_x = ((int64_t)(L_1|((int64_t)(L_2>>1))));
		int64_t L_3 = ___0_x;
		int64_t L_4 = ___0_x;
		___0_x = ((int64_t)(L_3|((int64_t)(L_4>>2))));
		int64_t L_5 = ___0_x;
		int64_t L_6 = ___0_x;
		___0_x = ((int64_t)(L_5|((int64_t)(L_6>>4))));
		int64_t L_7 = ___0_x;
		int64_t L_8 = ___0_x;
		___0_x = ((int64_t)(L_7|((int64_t)(L_8>>8))));
		int64_t L_9 = ___0_x;
		int64_t L_10 = ___0_x;
		___0_x = ((int64_t)(L_9|((int64_t)(L_10>>((int32_t)16)))));
		int64_t L_11 = ___0_x;
		int64_t L_12 = ___0_x;
		___0_x = ((int64_t)(L_11|((int64_t)(L_12>>((int32_t)32)))));
		int64_t L_13 = ___0_x;
		V_0 = ((int64_t)il2cpp_codegen_add(L_13, ((int64_t)1)));
		goto IL_003a;
	}

IL_003a:
	{
		int64_t L_14 = V_0;
		return L_14;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t EntityId_GetHashCode_m7B7BE3CC221C5EEAB3D2E64BD632F057E7931410_inline (EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->___m_Data;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t UnsafeUtility_SizeOf_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m97C7D5E5DE74DC60A0ECAA914830BEDF2C46ACAA_gshared_inline (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(uint8_t);
		return (int32_t)L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void UnsafeUtility_CopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mD5A26D4923C66071413642D33867560D087F543F_gshared_inline (uint8_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		uint8_t* L_0 = ___0_input;
		void* L_1 = ___1_ptr;
		UnsafeUtility_InternalCopyStructureToPtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_mFB7A3FF45C4521DC402828AC45C2220BC22ACF0A(L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t UnsafeUtility_SizeOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mED481D505BF43CBD96972069EDD4E3509BE84931_gshared_inline (const RuntimeMethod* method) 
{
	{
		uint32_t L_0 = sizeof(int32_t);
		return (int32_t)L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void UnsafeUtility_CopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m934C9E3CBBFA04ABEF4528E565C300B5DCF8C2D1_gshared_inline (int32_t* ___0_input, void* ___1_ptr, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		int32_t* L_0 = ___0_input;
		void* L_1 = ___1_ptr;
		UnsafeUtility_InternalCopyStructureToPtr_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m305D0FDE533B9169C6E61EDFEC9CFB0564E7B14B(L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 1));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB UnsafeUtility_ReadArrayElement_TisConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB_mB2E99510004443BADCB587A77413B3E0DCE17217_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB);
		ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB L_3 = (*(ConnectionId_tEA78CDC27F3C855E62B4A8046ECAFF5DB06128EB*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 UnsafeUtility_ReadArrayElement_TisDrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94_mFD7DBC1199616005466125347027F6201459B40D_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94);
		DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94 L_3 = (*(DrawKey_t3B4EE6E2AE19E9DD7C1731E60C6A7B39FEEA1C94*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 UnsafeUtility_ReadArrayElement_TisEntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8_mCA80AB19D82B40C3B0B1BC9A4BE2F87BAFD9BE64_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8);
		EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 L_3 = (*(EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 UnsafeUtility_ReadArrayElement_TisFixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952_mC7D1A0F4CA7AB74B987C2A810E03D044EFDF497B_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952);
		FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952 L_3 = (*(FixedString128Bytes_tEBC488E0CC30C6D842951A4E6F09AC58677F1952*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t UnsafeUtility_ReadArrayElement_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFBA66C06ECEB0A2BC1AAE634B6E6BB436F957084_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(int32_t);
		int32_t L_3 = (*(int32_t*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int64_t UnsafeUtility_ReadArrayElement_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m51BA37651B60ACB0F4F6867E07AB0C3E0046FC9C_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(int64_t);
		int64_t L_3 = (*(int64_t*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC UnsafeUtility_ReadArrayElement_TisNetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC_m35003350E9E0F53D03998488DCAEC3BA1AB15C7C_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC);
		NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC L_3 = (*(NetworkEndpoint_t0F60A2EF4E82ED5F26845E5537772D24C32426AC*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C UnsafeUtility_ReadArrayElement_TisRangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C_mFDF2C4591B5F4C0AF4424ED681225F9CF100C5BB_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C);
		RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C L_3 = (*(RangeKey_t6D4869B364ADC52DCAE541898513EF33CEE8878C*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 UnsafeUtility_ReadArrayElement_TisSharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692_mFA361CB1BDFC44955AC2650781EB1E111FB3768D_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692);
		SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692 L_3 = (*(SharedInstanceHandle_tC3415E5C355DF902D26BACE70048BB60426A3692*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint16_t UnsafeUtility_ReadArrayElement_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mDA4F2F1FC33E2F2F8CC3E761D626E638762C61E4_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(uint16_t);
		uint16_t L_3 = (*(uint16_t*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint32_t UnsafeUtility_ReadArrayElement_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mDA80BAFF55EA77496672B3B5B3804B55274B7E95_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(uint32_t);
		uint32_t L_3 = (*(uint32_t*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 UnsafeUtility_ReadArrayElement_TisUpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2_mC1917A7646F09213727BC23D1069EE21D2BF3920_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2);
		UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2 L_3 = (*(UpdatePipeline_t8D511DB7DDEC2170FC3349B879D1E46CD18C35D2*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C UnsafeUtility_ReadArrayElement_TisBatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C_mDE41FFE3160D153E47D8D93EF29E4B9C3DB23D20_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C);
		BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C L_3 = (*(BatchMaterialID_tF9E95833BB1C35A6D14D47CF4EC8A6EB7D15D65C*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 UnsafeUtility_ReadArrayElement_TisBatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0_mD0CAE37D791EB711A170B7F9BDCEAB6ED472C3E3_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0);
		BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0 L_3 = (*(BatchMeshID_t34167B7C1D0503C43A01A7BD213DFC367525D2A0*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 UnsafeUtility_ReadArrayElement_TisGPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38_mBA0DB77712EA2A549142CA67C7AE16B595A60205_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38);
		GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38 L_3 = (*(GPUDrivenPackedMaterialData_t9AFE25D18C8FA6EB6A69AEF342C2A19DA824CF38*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 UnsafeUtility_ReadArrayElement_TisGPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78_mF42F014B5AA9C633D3FCAB3FFF26646E54395BD8_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78);
		GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78 L_3 = (*(GPUInstanceIndex_t1B73FC29B273B0470A8E186E3C2F19CA6D8FBA78*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B UnsafeUtility_ReadArrayElement_TisInstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B_m86FC76E325E9D3B56133BB7A193BAF12B47FBCC1_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B);
		InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B L_3 = (*(InstanceHandle_tE8D892B001AFFDB8FA53EB19F2B356436AC36C3B*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E UnsafeUtility_ReadArrayElement_TisPerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E_m6992824D0CDD449EFB81329A07FAC0AC09F03218_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E);
		PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E L_3 = (*(PerCameraInstanceDataArrays_tA74F6F16B77B98DDC6E582A261525D7ECD12C07E*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E UnsafeUtility_ReadArrayElement_TisFixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E_m2ADCB1A0DE89B2E013B4BB7D300F81AECACFE8EA_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E);
		FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E L_3 = (*(FixedString512Bytes_t0C425B0F2C07FEA1642C32BF8559116DF2BFF50E*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 UnsafeUtility_ReadArrayElement_TisAnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46_m8460969BD9998B0DFF865186874B2414FEED12C8_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46);
		AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46 L_3 = (*(AnimatedFadeData_t965D6428A80522AB9EDFDF2671880282159C5E46*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 UnsafeUtility_ReadArrayElement_TisParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899_m14BBB3C75525EE04E460C0A3D179C85C5BC7C612_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899);
		ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899 L_3 = (*(ParameterSlice_t11C9B163EC6791F24FB25F285363BC41ECEBE899*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 UnsafeUtility_ReadArrayElement_TisBatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770_m80C61348EC8F9D4F73F0AAA4A69A028E6D599DCD_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770);
		BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770 L_3 = (*(BatchID_t884E3C204D1F4C18FFD2361FC14A1E64CFBD8770*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 UnsafeUtility_ReadArrayElement_TisGeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271_m2A66DBE83F9498AC60B42C9A0B1C7E4401B4CDCC_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271);
		GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271 L_3 = (*(GeometryPoolHandle_t7EF37F11E23B05A26C1F72855D269B7A772B2271*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool UnsafeUtility_ReadArrayElement_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mF765CB2E5FD631DFB79C25DE656F3C1BB359B995_gshared_inline (void* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		void* L_0 = ___0_source;
		int32_t L_1 = ___1_index;
		uint32_t L_2 = sizeof(bool);
		bool L_3 = (*(bool*)((void*)il2cpp_codegen_add((intptr_t)L_0, ((intptr_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_1), ((int64_t)((int32_t)L_2))))))));
		return L_3;
	}
}
