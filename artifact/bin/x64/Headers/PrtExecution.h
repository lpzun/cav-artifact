#ifndef PRT_EXECUTION_H
#define PRT_EXECUTION_H

#include "Prt.h"

#ifdef __cplusplus
extern "C"{
#endif

	extern PRT_EVENTDECL _P_EVENT_NULL_STRUCT;
	extern PRT_EVENTDECL _P_EVENT_HALT_STRUCT;

	//
	// Max call stack size of each machine
	//
#define PRT_MAX_STATESTACK_DEPTH 16

#define PRT_MAX_FUNSTACK_DEPTH 16

#define PRT_MAX_EVENTSTACK_DEPTH 10

	//
	// Initial length of the event queue for each machine
	//
#define PRT_QUEUE_LEN_DEFAULT 64

    typedef struct PRT_COOPERATIVE_SCHEDULER
    {
        PRT_SEMAPHORE           workAvailable;      /* semaphore to signal blocked PrtRunProcess threads */
        PRT_UINT32              threadsWaiting;     /* number of PrtRunProcess threads waiting for work */
        PRT_SEMAPHORE           allThreadsStopped;  /* all PrtRunProcess threads have terminated */
    } PRT_COOPERATIVE_SCHEDULER;

	typedef struct PRT_PROCESS_PRIV {
		PRT_GUID				guid;
		PRT_ERROR_FUN	        errorHandler;
		PRT_LOG_FUN				logHandler;
		PRT_RECURSIVE_MUTEX		processLock;
		PRT_UINT32				numMachines;
		PRT_UINT32				machineCount;
		PRT_MACHINEINST			**machines;
        PRT_BOOLEAN             terminating;        /* PrtStopProcess has been called */
        PRT_SCHEDULINGPOLICY    schedulingPolicy;
        void*                   schedulerInfo;      /* for example, this could be PRT_COOPERATIVE_SCHEDULER */

	} PRT_PROCESS_PRIV;

	typedef enum PRT_LASTOPERATION
	{
		ReturnStatement,
		PopStatement,
		RaiseStatement,
		GotoStatement
	} PRT_LASTOPERATION;

    typedef enum PRT_NEXTOPERATION
    {
        EntryOperation,
        DequeueOperation,
        HandleEventOperation,
        ReceiveOperation
    } PRT_NEXTOPERATION;

	typedef enum PRT_EXITREASON
	{
		NotExit,
		OnTransition,
		OnTransitionAfterExit,
		OnPopStatement,
		OnGotoStatement,
		OnUnhandledEvent
	} PRT_EXITREASON;

	typedef struct PRT_EVENT
	{
		PRT_VALUE *trigger;
		PRT_VALUE *payload;
		PRT_MACHINESTATE state;
	} PRT_EVENT;

	typedef struct PRT_EVENTQUEUE
	{
		PRT_UINT32		 eventsSize;
		PRT_EVENT		*events;
		PRT_UINT32		 headIndex;
		PRT_UINT32		 tailIndex;
		PRT_UINT32		 size;
	} PRT_EVENTQUEUE;

	typedef struct PRT_STATESTACK_INFO
	{
		PRT_UINT32			stateIndex;
		PRT_UINT32*			inheritedDeferredSetCompact;
		PRT_UINT32*			inheritedActionSetCompact;
	} PRT_STATESTACK_INFO;

	typedef struct PRT_STATESTACK
	{
		PRT_STATESTACK_INFO stateStack[PRT_MAX_STATESTACK_DEPTH];
		PRT_UINT16			length;
	} PRT_STATESTACK;

	typedef struct PRT_FUNSTACK_INFO
	{
		PRT_FUNDECL			*funDecl;
		PRT_VALUE			**locals;
		PRT_BOOLEAN			freeLocals; 
		PRT_VALUE			***refArgs;
		PRT_UINT16			returnTo;
		PRT_CASEDECL		*rcase;
	} PRT_FUNSTACK_INFO;

	typedef struct PRT_FUNSTACK
	{
		PRT_FUNSTACK_INFO	funs[PRT_MAX_FUNSTACK_DEPTH];
		PRT_UINT16			length;
	} PRT_FUNSTACK;

	typedef struct PRT_EVENTSTACK
	{
		PRT_EVENT			events[PRT_MAX_EVENTSTACK_DEPTH];
		PRT_UINT16			length;
	} PRT_EVENTSTACK;

	typedef struct PRT_MACHINEINST_PRIV {
		PRT_PROCESS		    *process;
		PRT_UINT32			instanceOf;
		PRT_VALUE			*id;
		PRT_VALUE           *recvMap;
		PRT_VALUE			**varValues;
		PRT_RECURSIVE_MUTEX stateMachineLock;
		PRT_BOOLEAN			isRunning;
        PRT_NEXTOPERATION   nextOperation;
		PRT_EXITREASON		exitReason;
		PRT_UINT32			eventValue;
		PRT_BOOLEAN			isHalted;
		PRT_UINT32			currentState;
		PRT_RECEIVEDECL		*receive;
		PRT_STATESTACK		callStack;
		PRT_FUNSTACK		funStack;
		PRT_UINT32			destStateIndex;
		PRT_VALUE			*currentTrigger;
		PRT_VALUE			*currentPayload;
		PRT_EVENTQUEUE		eventQueue;
		PRT_LASTOPERATION	lastOperation;
		PRT_UINT32          *inheritedDeferredSetCompact;
		PRT_UINT32          *currentDeferredSetCompact;
		PRT_UINT32          *inheritedActionSetCompact;
		PRT_UINT32          *currentActionSetCompact;
		PRT_UINT32			interfaceBound;
	} PRT_MACHINEINST_PRIV;

	/** Sets a global variable to variable
	* @param[in,out] context The context to modify.
	* @param[in] varIndex The index of the variable to modify.
	* @param[in] value The value to set. (Will be cloned)
	*/
	PRT_API void PRT_CALL_CONV PrtSetGlobalVar(_Inout_ PRT_MACHINEINST_PRIV * context, _In_ PRT_UINT32 varIndex, _In_ PRT_VALUE * value);

	/** Sets a global variable to variable
	* @param[in,out] context The context to modify.
	* @param[in] varIndex The index of the variable to modify.
	* @param[in] status Indicates whether this operation is move or swap
	* @param[in,out] value The pointer to the value to move or swap
	* @param[in]     type The type of data pointed to by value
	*/
	PRT_API void PRT_CALL_CONV PrtSetGlobalVarLinear(_Inout_ PRT_MACHINEINST_PRIV * context, _In_ PRT_UINT32 varIndex, _In_ PRT_FUN_PARAM_STATUS status, _Inout_ PRT_VALUE ** value, _In_ PRT_TYPE *type);

	/** Sets a global variable to variable
	* @param[in,out] context The context to modify.
	* @param[in] varIndex The index of the variable to modify.
	* @param[in] value The value to set. (Will be cloned if cloneValue is PRT_TRUE)
	* @param[in] cloneValue Only set to PRT_FALSE if value will be forever owned by this machine.
	*/
	PRT_API void PRT_CALL_CONV PrtSetGlobalVarEx(_Inout_ PRT_MACHINEINST_PRIV * context, _In_ PRT_UINT32 varIndex, _In_ PRT_VALUE * value, _In_ PRT_BOOLEAN cloneValue);

	PRT_MACHINEINST_PRIV *
		PrtMkMachinePrivate(
		_Inout_  PRT_PROCESS_PRIV		*process,
		_In_  PRT_UINT32				interfaceName,
		_In_  PRT_UINT32				instanceOf,
		_In_  PRT_VALUE					*payload
		);

	PRT_API void PRT_CALL_CONV PrtSetLocalVarLinear(
		_Inout_ PRT_VALUE **locals,
		_In_ PRT_UINT32 varIndex,
		_In_ PRT_FUN_PARAM_STATUS status,
		_Inout_ PRT_VALUE **value,
		_In_ PRT_TYPE *type
	);

	PRT_API void PRT_CALL_CONV PrtSetLocalVarEx(
		_Inout_ PRT_VALUE **locals,
		_In_ PRT_UINT32 varIndex,
		_In_ PRT_VALUE *value,
		_In_ PRT_BOOLEAN cloneValue
		);

	PRT_VALUE *MakeTupleFromArray(
		_In_ PRT_TYPE *tupleType, 
		_In_ PRT_VALUE **elems
		);
	
	void
		PrtSendPrivate(
		_In_ PRT_MACHINESTATE           *state,
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_VALUE					*event,
		_In_ PRT_VALUE					*payload
		);

	PRT_API void PRT_CALL_CONV
		PrtGoto(
			_Inout_ PRT_MACHINEINST_PRIV		*context,
			_In_ PRT_UINT32						destStateIndex,
			_In_ PRT_UINT32						numArgs,
			...
		);
	
	PRT_API void PRT_CALL_CONV
		PrtRaise(
		_Inout_ PRT_MACHINEINST_PRIV		*context,
		_In_ PRT_VALUE						*event,
		_In_ PRT_UINT32						numArgs,
		...
		);

	void
		PrtPushState(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_In_	PRT_UINT32			stateIndex
		);

	PRT_API void PRT_CALL_CONV
		PrtPop(
		_Inout_ PRT_MACHINEINST_PRIV		*context
		);

	PRT_BOOLEAN
		PrtPopState(
		_Inout_ PRT_MACHINEINST_PRIV		*context,
		_In_ PRT_BOOLEAN				isPopStatement
		);

	FORCEINLINE
		void
		PrtRunExitFunction(
		_In_ PRT_MACHINEINST_PRIV			*context
		);

	FORCEINLINE
		void
		PrtRunTransitionFunction(
			_In_ PRT_MACHINEINST_PRIV			*context,
			_In_ PRT_UINT32						transIndex
		);

	PRT_UINT32
		PrtFindTransition(
		_In_ PRT_MACHINEINST_PRIV		*context,
		_In_ PRT_UINT32					eventIndex
		);

	void
		PrtTakeTransition(
		_Inout_ PRT_MACHINEINST_PRIV		*context,
		_In_ PRT_UINT32					eventIndex
		);

	PRT_BOOLEAN
		PrtDequeueEvent(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_Inout_ PRT_FUNSTACK_INFO		*frame
		);

	FORCEINLINE
		PRT_STATEDECL *
		PrtGetCurrentStateDecl(
		_In_ PRT_MACHINEINST_PRIV			*context
		);

	PRT_TYPE*
		PrtGetPayloadType(
		_In_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_VALUE				*event
		);

	FORCEINLINE
		PRT_UINT16
		PrtGetPackSize(
		_In_ PRT_MACHINEINST_PRIV			*context
		);

	FORCEINLINE
		PRT_SM_FUN
		PrtGetEntryFunction(
		_In_ PRT_MACHINEINST_PRIV		*context
		);

	FORCEINLINE
		PRT_SM_FUN
		PrtGetExitFunction(
		_In_ PRT_MACHINEINST_PRIV		*context
		);

	FORCEINLINE
		PRT_DODECL*
		PrtGetAction(
		_In_ PRT_MACHINEINST_PRIV		*context,
		_In_ PRT_UINT32					currEvent
		);

	FORCEINLINE
		PRT_UINT32*
		PrtGetDeferredPacked(
		_In_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_UINT32				stateIndex
		);

	FORCEINLINE
		PRT_UINT32*
		PrtGetActionsPacked(
		_In_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_UINT32				stateIndex
		);

	FORCEINLINE
		PRT_UINT32*
		PrtGetTransitionsPacked(
		_In_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_UINT32				stateIndex
		);

	FORCEINLINE
		PRT_TRANSDECL*
		PrtGetTransitionTable(
		_In_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_UINT32				stateIndex,
		_Out_ PRT_UINT32			*nTransitions
		);

	PRT_BOOLEAN
		PrtAreGuidsEqual(
		_In_ PRT_GUID guid1,
		_In_ PRT_GUID guid2
		);

	PRT_BOOLEAN
		PrtIsEventMaxInstanceExceeded(
		_In_ PRT_EVENTQUEUE			*queue,
		_In_ PRT_UINT32				eventIndex,
		_In_ PRT_UINT32				maxInstances
		);

	FORCEINLINE
		PRT_BOOLEAN
		PrtStateHasDefaultTransitionOrAction(
		_In_ PRT_MACHINEINST_PRIV			*context
		);

	FORCEINLINE
		PRT_BOOLEAN
		PrtIsSpecialEvent(
		_In_ PRT_VALUE * event
		);

	FORCEINLINE
		PRT_BOOLEAN
		PrtIsEventReceivable(
		_In_ PRT_MACHINEINST_PRIV *context,
		_In_ PRT_UINT32		eventIndex
		);

	FORCEINLINE
		PRT_BOOLEAN
		PrtIsEventDeferred(
		_In_ PRT_UINT32		eventIndex,
		_In_ PRT_UINT32*		defSet
		);

	FORCEINLINE
		PRT_BOOLEAN
		PrtIsActionInstalled(
		_In_ PRT_UINT32		eventIndex,
		_In_ PRT_UINT32*	actionSet
		);

	FORCEINLINE
		PRT_BOOLEAN
		PrtIsTransitionPresent(
		_In_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_UINT32				eventIndex
		);

	PRT_BOOLEAN
		PrtIsPushTransition(
		_In_ PRT_MACHINEINST_PRIV		*context,
		_In_ PRT_UINT32					event
		);

	PRT_UINT32 *
		PrtClonePackedSet(
		_In_ PRT_UINT32 *				packedSet,
		_In_ PRT_UINT32					size
		);

	void
		PrtUpdateCurrentActionsSet(
		_Inout_ PRT_MACHINEINST_PRIV			*context
		);

	void
		PrtUpdateCurrentDeferredSet(
		_Inout_ PRT_MACHINEINST_PRIV			*context
		);

	void
		PrtResizeEventQueue(
		_Inout_ PRT_MACHINEINST_PRIV *context
		);

	void
		PrtHaltMachine(
		_Inout_ PRT_MACHINEINST_PRIV			*context
		);

	void
		PrtCleanupMachine(
		_Inout_ PRT_MACHINEINST_PRIV			*context
		);

	PRT_API void
		PrtHandleError(
		_In_ PRT_STATUS ex,
		_In_ PRT_MACHINEINST_PRIV *context
		);

	void PRT_CALL_CONV
		PrtAssertDefaultFn(
		_In_ PRT_INT32 condition,
		_In_opt_z_ PRT_CSTRING message
		);

	PRT_API void PRT_CALL_CONV
		PrtUpdateAssertFn(
		PRT_ASSERT_FUN assertFn
		);

	PRT_API void PRT_CALL_CONV
		PrtUpdatePrintFn(
		PRT_PRINT_FUN printFn
		);

	void PRT_CALL_CONV
		PrtPrintfDefaultFn(
		_In_opt_z_ PRT_CSTRING message
		);

	PRT_API void
		PrtLog(
		_In_ PRT_STEP step,
		_In_ PRT_MACHINESTATE* state,
		_In_ PRT_MACHINEINST_PRIV *receiver,
		_In_ PRT_VALUE* eventId,
		_In_ PRT_VALUE* payload
		);

	PRT_API void
		PrtCheckIsLocalMachineId(
		_In_ PRT_MACHINEINST *context,
		_In_ PRT_VALUE *id
		);

	PRT_VALUE *
		PrtGetCurrentTrigger(
		_Inout_ PRT_MACHINEINST_PRIV	*context
		);

	PRT_VALUE *
		PrtGetCurrentPayload(
		_Inout_ PRT_MACHINEINST_PRIV		*context
		);

	PRT_FUNSTACK_INFO *
		PrtTopOfFunStack(
		_In_ PRT_MACHINEINST_PRIV	*context
		);

	PRT_FUNSTACK_INFO *
		PrtBottomOfFunStack(
		_In_ PRT_MACHINEINST_PRIV	*context
		);

	void
		PrtPushNewEventHandlerFrame(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_FUNDECL				*funDecl,
		_In_ PRT_FUN_PARAM_STATUS       payloadStatus, 
		_In_ PRT_VALUE					**locals
		);

	void
		PrtPushNewFrame(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_BOOLEAN				isFunApp,
		_In_ PRT_FUNDECL				*funDecl,
		...
		);

	PRT_API void
		PrtPushFrame(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_FUNSTACK_INFO *funStackInfo
		);

	PRT_API void
		PrtPopFrame(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_Inout_ PRT_FUNSTACK_INFO *funStackInfo
		);

	PRT_API void
		PrtFreeLocals(
		_In_ PRT_MACHINEINST_PRIV		*context,
		_Inout_ PRT_FUNSTACK_INFO		*frame
		);

	PRT_API PRT_VALUE *
		PrtWrapFunStmt(
		_Inout_ PRT_FUNSTACK_INFO		*frame,
		_In_ PRT_UINT16					funCallIndex,
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_In_ PRT_FUNDECL				*funDecl
		);

	PRT_API PRT_BOOLEAN
		PrtReceive(
		_Inout_ PRT_MACHINEINST_PRIV	*context,
		_Inout_ PRT_FUNSTACK_INFO		*funStackInfo,
		_In_ PRT_UINT16					receiveIndex
		);

	PRT_API void
		PrtRunStateMachine(
		_Inout_ PRT_MACHINEINST_PRIV	    *context
		);

	PRT_API void PRT_CALL_CONV PrtEnqueueInOrder(
		_In_ PRT_VALUE					*source,
		_In_ PRT_INT64					seqNum,
		_Inout_ PRT_MACHINEINST_PRIV	*machine,
		_In_ PRT_VALUE					*evt,
		_In_ PRT_VALUE					*payload
		);

#ifdef __cplusplus
}
#endif
#endif