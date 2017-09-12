"use strict";

/**
 * 注意成员名前面有下划线表示是私有成员，不建议外部访问，如果想提供给外部只读或只写，那写相应的getter或setter
 * 如果要保证一个用户的数据写入到数据库是按调用的顺序来的，请在连接字符串里设置poolSize=1或maxPoolSize=1
 * 驱动的API文档：{@link http://mongodb.github.io/node-mongodb-native/2.0/api/}
 * 错误码定义：{@link https://github.com/mongodb/mongo/blob/master/src/mongo/base/error_codes.err}
 */

////////////外部模块////////////
var MongoDB = require("mongodb");
var MongoClient = MongoDB.MongoClient;
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../config");
var appUtil = require("./appUtil");
var logUtil = require("./logUtil");

////////////常量定义////////////
const ERROR_CODE = {
    OK:0,
    InternalError:1,
    BadValue:2,
    OBSOLETE_DuplicateKey:3,
    NoSuchKey:4,
    GraphContainsCycle:5,
    HostUnreachable:6,
    HostNotFound:7,
    UnknownError:8,
    FailedToParse:9,
    CannotMutateObject:10,
    UserNotFound:11,
    UnsupportedFormat:12,
    Unauthorized:13,
    TypeMismatch:14,
    Overflow:15,
    InvalidLength:16,
    ProtocolError:17,
    AuthenticationFailed:18,
    CannotReuseObject:19,
    IllegalOperation:20,
    EmptyArrayOperation:21,
    InvalidBSON:22,
    AlreadyInitialized:23,
    LockTimeout:24,
    RemoteValidationError:25,
    NamespaceNotFound:26,
    IndexNotFound:27,
    PathNotViable:28,
    NonExistentPath:29,
    InvalidPath:30,
    RoleNotFound:31,
    RolesNotRelated:32,
    PrivilegeNotFound:33,
    CannotBackfillArray:34,
    UserModificationFailed:35,
    RemoteChangeDetected:36,
    FileRenameFailed:37,
    FileNotOpen:38,
    FileStreamFailed:39,
    ConflictingUpdateOperators:40,
    FileAlreadyOpen:41,
    LogWriteFailed:42,
    CursorNotFound:43,
    UserDataInconsistent:45,
    LockBusy:46,
    NoMatchingDocument:47,
    NamespaceExists:48,
    InvalidRoleModification:49,
    ExceededTimeLimit:50,
    ManualInterventionRequired:51,
    DollarPrefixedFieldName:52,
    InvalidIdField:53,
    NotSingleValueField:54,
    InvalidDBRef:55,
    EmptyFieldName:56,
    DottedFieldName:57,
    RoleModificationFailed:58,
    CommandNotFound:59,
    OBSOLETE_DatabaseNotFound:60,
    ShardKeyNotFound:61,
    OplogOperationUnsupported:62,
    StaleShardVersion:63,
    WriteConcernFailed:64,
    MultipleErrorsOccurred:65,
    ImmutableField:66,
    CannotCreateIndex:67,
    IndexAlreadyExists:68,
    AuthSchemaIncompatible:69,
    ShardNotFound:70,
    ReplicaSetNotFound:71,
    InvalidOptions:72,
    InvalidNamespace:73,
    NodeNotFound:74,
    WriteConcernLegacyOK:75,
    NoReplicationEnabled:76,
    OperationIncomplete:77,
    CommandResultSchemaViolation:78,
    UnknownReplWriteConcern:79,
    RoleDataInconsistent:80,
    NoMatchParseContext:81,
    NoProgressMade:82,
    RemoteResultsUnavailable:83,
    DuplicateKeyValue:84,
    IndexOptionsConflict:85,
    IndexKeySpecsConflict:86,
    CannotSplit:87,
    SplitFailed:88,
    NetworkTimeout:89,
    CallbackCanceled:90,
    ShutdownInProgress:91,
    SecondaryAheadOfPrimary:92,
    InvalidReplicaSetConfig:93,
    NotYetInitialized:94,
    NotSecondary:95,
    OperationFailed:96,
    NoProjectionFound:97,
    DBPathInUse:98,
    WriteConcernNotDefined:99,
    CannotSatisfyWriteConcern:100,
    OutdatedClient:101,
    IncompatibleAuditMetadata:102,
    NewReplicaSetConfigurationIncompatible:103,
    NodeNotElectable:104,
    IncompatibleShardingMetadata:105,
    DistributedClockSkewed:106,
    LockFailed:107,
    InconsistentReplicaSetNames:108,
    ConfigurationInProgress:109,
    CannotInitializeNodeWithData:110,
    NotExactValueField:111,
    WriteConflict:112,
    InitialSyncFailure:113,
    InitialSyncOplogSourceMissing:114,
    CommandNotSupported:115,
    DocTooLargeForCapped:116,
    ConflictingOperationInProgress:117,
    NamespaceNotSharded:118,
    InvalidSyncSource:119,
    OplogStartMissing:120,
    DocumentValidationFailure:121,
    OBSOLETE_ReadAfterOptimeTimeout:122,
    NotAReplicaSet:123,
    IncompatibleElectionProtocol:124,
    CommandFailed:125,
    RPCProtocolNegotiationFailed:126,
    UnrecoverableRollbackError:127,
    LockNotFound:128,
    LockStateChangeFailed:129,
    SymbolNotFound:130,
    RLPInitializationFailed:131,
    ConfigServersInconsistent:132,
    FailedToSatisfyReadPreference:133,
    ReadConcernMajorityNotAvailableYet:134,
    StaleTerm:135,
    CappedPositionLost:136,
    IncompatibleShardingConfigVersion:137,
    RemoteOplogStale:138,
    JSInterpreterFailure:139,
    InvalidSSLConfiguration:140,
    SSLHandshakeFailed:141,
    JSUncatchableError:142,
    CursorInUse:143,
    IncompatibleCatalogManager:144,
    PooledConnectionsDropped:145,
    ExceededMemoryLimit:146,
    ZLibError:147,
    ReadConcernMajorityNotEnabled:148,
    NoConfigMaster:149,
    StaleEpoch:150,
    OperationCannotBeBatched:151,
    OplogOutOfOrder:152,
    RecvStaleConfig:9996,
    NotMaster:10107,
    CannotGrowDocumentInCappedNamespace:10003,
    DuplicateKey:11000,
    InterruptedAtShutdown:11600,
    Interrupted:11601,
    InterruptedDueToReplStateChange:11602,
    OutOfDiskSpace:14031,
    KeyTooLong:17280,
    BackgroundOperationInProgressForDatabase:12586,
    BackgroundOperationInProgressForNamespace:12587,
    NotMasterOrSecondary:13436,
    NotMasterNoSlaveOk:13435,
    ShardKeyTooBig:13334,
    SendStaleConfig:13388,
    DatabaseDifferCase:13297,
    PrepareConfigsFailed:13104
};

////////////模块内数据////////////
/**
 *
 * @type {MyDB[]}
 */
var myDBPool = [];

////////////自定义类////////////
/**
 * 数据库包装类
 * @class
 */
class MyDB {
    /**
     *
     * @param {number} connId
     * @param {string} connUrl
     */
    constructor(connId, connUrl) {
        this._connId = connId;
        /**
         * @type {Db}
         */
        this._mongoDB = null;
        this._connUrl = connUrl;
        /**
         * @type {object.<string, MyCollection>}
         */
        this._collectionMap = {};
        /**
         * 为了防止同时有多个协程在重连数据库，用这个锁来只允许一个重连
         * @type {boolean}
         */
        this._onConnecting = false;
        /**
         * 为了让等待重连的数据库知道有连接重试失败了
         * @type {Error}
         */
        this._lastConnError = null;
        /**
         * 等待重连的协程编号队列
         * @type {number[]}
         */
        this._waitingConnQueue = [];
        /**
         * 等待重连的协程编号种子
         * @type {number}
         */
        this._waitingConnNumber= 0;
        /**
         * 标记连接为了退出进程而关闭，这时不可以再重连了
         * @type {boolean}
         */
        this._closeForExiting = false;
    }

    logInfo(msg) {
        logUtil.info("[连接" + this._connId + "]" + msg);
    }

    logError(msg, err) {
        logUtil.error("[连接" + this._connId + "]" + (msg || ""), err);
    }

    getMongoDB() {
        return this._mongoDB;
    }

    getConnId() {
        return this._connId;
    }

    static incPendingOp() {
        ++MyDB._pendingOpCnt;
    }

    static decPendingOp() {
        --MyDB._pendingOpCnt;
        if (MyDB._pendingOpCnt < 0) {
            MyDB._pendingOpCnt = 0;
            logUtil.error(null, new Error("pendingOpCnt小于0了"));
        }
    }

    static getPendingOpCnt() {
        return MyDB._pendingOpCnt;
    }

    /**
     * 现在开始连接
     * @throws {MongoError}
     */
    connect() {
        return MyDB._ConnectCoroutine(this);
    }

    /**
     * 关闭数据库连接
     * @param {boolean?} forExiting - 标记是否永久关闭本连接，也就是不可以再重连
     */
    close(forExiting) {
        if (this._mongoDB !== null) {
            this.logInfo("断开数据库连接");
            this._mongoDB.close(false);
            this._mongoDB = null;
            this._closeForExiting = this._closeForExiting || forExiting; //不直接使用forExiting，是因为可能后面又调用不带参数或参数为false的close
            //数据库对象无效了，把集合类也重置一下，以便重连后重新获取
            for (var colName in this._collectionMap) {
                if (this._collectionMap.hasOwnProperty(colName))
                {
                    var myCol = this._collectionMap[colName];
                    myCol._mongoCol = null;
                }
            }
        }
    }

    /**
     * 是否已连接
     * @returns {boolean}
     */
    isConnect() {
        return this._mongoDB !== null;
    }

    /**
     * 返回指定名字的集合类，注意名字的大小写
     * @param {string} colName
     * @return {MyCollection}
     */
    collection(colName) {
        if (!this._collectionMap[colName])
            this._collectionMap[colName] = new MyCollection(colName, this);
        return this._collectionMap[colName];
    }

    /**
     * 检查是否连接，如果未连接，则尝试连接，并会失败重试
     * @param {boolean} force - 用于操作数据发生异常时告诉本函数要用PING命令检查已连接中的连接是否失效还是其它原因（比如参数不对）导致的错误
     * @return {boolean} true表示没发生重连，false表示发生了重连，多次重连失败直接抛出异常
     * @throws {MongoError}
     */
    checkConn(force) {
        return MyDB._CheckConnCoroutine(this, force);
    }
}
//静态成员变量
MyDB._pendingOpCnt = 0;
MyDB._ConnectCoroutine = Promise.coroutine(function * (self) {
    //已永久关闭的连接不能再重连
    if (self._closeForExiting)
    {
        var err = new Error("连接已永久关闭，不可以再重连");
        self.logError(null, err);
        throw err;
    }

    //已在连接中？那就本协程不连接了，光等就行了
    if (self._onConnecting) {
        //生成种了，加入队列
        var waitingNumber = ++self._waitingConnNumber;
        self._waitingConnQueue.push(waitingNumber);
        //不断等正在连接中的协程结束连接
        //这里还要等等待队列轮到自己执行
        while (self._onConnecting || (self._waitingConnQueue.length > 0 && self._waitingConnQueue[0] !== waitingNumber))
        {
            //注意，这里会导致执行重连成功后，数据操作执行顺序不对
            yield Promise.delay(10);
        }
        //终于轮到自己了
        //如果队首是自己，就把自己出队
        if (self._waitingConnQueue.length > 0 && self._waitingConnQueue[0] === waitingNumber)
        {
            self._waitingConnQueue.shift();
        }
        //居然有错误？那就抛出错误
        if (self._lastConnError)
            throw self._lastConnError;
        //本协程只负责等，不能再进入下面了
        return;
    }

    //成功进入重连代码，清空最后错误
    self._lastConnError = null;

    //设置在连接中
    self._onConnecting = true;

    //尝试关闭先前的连接
    self.close();

    //开启循环，连接失败就多尝试几次
    var retryCount = 1;
    var retryDelay = appCfg.minReconnInv;
    do
    {
        var errObj = null;
        try {
            if (retryCount > 1) {
                self.logInfo((retryDelay / 1000) + "秒后重试");

                yield Promise.delay(retryDelay);
                retryDelay = Math.min(appCfg.maxReconnInv, retryDelay + appCfg.incReconnInv);

                self.logInfo("第" + retryCount + "次尝试连接数据");
            }
            else {
                self.logInfo("开始连接数据库");
            }

            self.logInfo("连接字符串：" + self._connUrl);
            self._mongoDB = yield MongoClient.connect(self._connUrl);
            self.logInfo("成功连接数据库");
        }
        catch (err) {
            errObj = err;
            ++retryCount;
            self.logError("连接数据库失败", errObj);
        }

        var retryCountLimit = appUtil.isProcessExiting() ? appCfg.maxRetryCntExiting : appCfg.maxRetryCntRunning;
    }
    while (errObj && retryCount <= retryCountLimit);

    //设置不在连接中
    self._onConnecting = false;

    //超过重试次数就抛出异常
    if (errObj) {
        self._lastConnError = errObj;
        throw errObj;
    }
    else {
        //成功连接，清空最后错误
        self._lastConnError = null;
    }
});
MyDB._CheckConnCoroutine = Promise.coroutine(function * (self, force) {
    //已连接？不用再连接了
    if (self.isConnect()) {
        //不用强制检查连接？那就直接返回
        if (!force)
            return true;
        try {
            yield self._mongoDB.command({ping: 1});
            //能到这里，说明ping成功，连接正常
            return true;
        }
        catch (err) {
            //发生错误？那就进入下面重连吧
            self.logError("数据库连接失效，准备重连", err);
        }
    }

    //未连接？那就连接吧，如果多次尝试连接失败，会抛出错误
    yield self.connect();
    return false;
});

/**
 * 数据集合包装类
 * @class
 */
class MyCollection {
    /**
     *
     * @param {string} colName
     * @param {MyDB} myDB
     */
    constructor(colName, myDB) {
        this._colName = colName;
        this._myDB = myDB;
        /**
         *
         * @type {Collection}
         */
        this._mongoCol = null;
    }

    /**
     * 查询数据，返回数组或对象
     * 如果出错会抛出异常
     * 不要用这个函数查询大量数据，使用{@see findCursor}
     * @param {object?} query - 查询条件，比如{a:1,b:1}，可以不填这个参数，则表示查询全部
     * @param {object?} fields - 如果填了，就限制返回的字段，比如{_id:0,b:1}，0表示不获取，1表示获取
     * @param {object?} options - 相关选项，比如{skip:1,limit:1}
     * @param {number?} options.skip - 如果填了，就跳过多少条数据
     * @param {number?} options.limit - 如果填了，就限制多少条数据
     * @param {boolean?} [options.returnObject=false] - 当limit为1时，returnObject为true时，返回的是object，如果数据没空，返回null
     * @param {(object|array.<array>)?} options.sort - 如果填了，使用排序，注意格式是对象或二维数组，比如{a:1,b:1}或[["a",1],["b",1]]，1表示升序，-1表示降序
     * @param {boolean?} debug - 是否打印查询结果，用于调试
     * @return {object[]|object|null} 结果数据，一般返回数组，当limit === 1、returnObject === true时，返回对象或null(没数据时)
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    findArray(query, fields, options, debug) {
        return MyCollection._findArrayCoroutine(this, "MyCollection~findArray", false, query, fields, options, debug);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 查询数据，返回数组或对象
     * 不要用这个函数查询大量数据，使用{@see findCursor}
     * @param {object?} query - 查询条件，比如{a:1,b:1}，可以不填这个参数，则表示查询全部
     * @param {object?} fields - 如果填了，就限制返回的字段，比如{_id:0,b:1}，0表示不获取，1表示获取
     * @param {object?} options - 相关选项，比如{skip:1,limit:1}
     * @param {number?} options.skip - 如果填了，就跳过多少条数据
     * @param {number?} options.limit - 如果填了，就限制多少条数据
     * @param {boolean?} [options.returnObject=false] - 当limit为1时，returnObject为true时，返回的是object，如果数据没空，返回null
     * @param {(object|array.<array>)?} options.sort - 如果填了，使用排序，注意格式是对象或二维数组，比如{a:1,b:1}或[["a",1],["b",1]]，1表示升序，-1表示降序
     * @param {boolean?} debug - 是否打印查询结果，用于调试
     * @return {object[]|object|null} 结果数据，一般返回数组，当limit === 1、returnObject === true时，返回对象或null(没数据时)
     */
    findArrayNoThrow(query, fields, options, debug) {
        return MyCollection._findArrayCoroutine(this, "MyCollection~findArrayNoThrow", true, query, fields, options, debug);
    }

    /**
     * 查询数据，直接返回MongoDB游标，没有使用协程，也没有加减延迟操作计数
     * 本函数使用场景受限，不要过多使用，一般用来长时间定时任务
     * 如果数据库连接无效，【不会重连数据库】，直接返回null
     * 对于迭代过程中，连接失效【也不会重连数据库】
     * 返回的Cursor可以如下迭代
     * @example
     *  //开始迭代
     *  while(yield cursor.hasNext()) {
     *      var doc = yield cursor.next();
     *  }
     *  //注意关闭
     *  cursor.close();
     * @param {object?} query - 查询条件，比如{a:1,b:1}，可以不填这个参数，则表示查询全部
     * @param {object?} fields - 如果填了，就限制返回的字段，比如{_id:0,b:1}，0表示不获取，1表示获取
     * @param {object?} options - 相关选项，比如{skip:1,limit:1}
     * @param {number?} options.skip - 如果填了，就跳过多少条数据
     * @param {number?} options.limit - 如果填了，就限制多少条数据
     * @param {(object|array.<array>)?} options.sort - 如果填了，使用排序，注意格式是对象或二维数组，比如{a:1,b:1}或[["a",1],["b",1]]，1表示升序，-1表示降序
     * @return {Cursor|null} MongoDB的游标
     */
    findCursor(query, fields, options) {
        if (!this._myDB)
            return null;

        if (!this._mongoCol)
            this._mongoCol = this._myDB.getMongoDB().collection(this._colName);

        return this._mongoCol.find(query, fields, options);
    }

    /**
     * 查询数据，返回对象，如果没有数据，返回null，如果出错会抛出异常，这里用了本类另一方法，所以不用增减操作计数
     * @param {object?} query - 查询条件，比如{a:1,b:1}，可以不填这个参数，则表示查询第一个
     * @param {object?} fields - 如果填了，就限制返回的字段，比如{_id:0,b:1}，0表示不获取，1表示获取
     * @return {object|null} 有数据就返回object，否则是null
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    findOne(query, fields) {
        return this.findArray(query, fields, {limit: 1, returnObject: true});
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 查询数据，返回对象，如果没有数据，返回null，这里用了本类另一方法，所以不用增减操作计数
     * @param {object?} query - 查询条件，比如{a:1,b:1}，可以不填这个参数，则表示查询第一个
     * @param {object?} fields - 如果填了，就限制返回的字段，比如{_id:0,b:1}，0表示不获取，1表示获取
     * @return {object|null} 有数据就返回object，否则是null
     */
    findOneNoThrow(query, fields) {
        return this.findArrayNoThrow(query, fields, {limit: 1, returnObject: true});
    }

    /**
     * 执行聚合操作，返回数组
     * 如果出错会抛出异常
     * 不要用这个函数查询大量数据，使用{@see aggregateCursor}
     * 对于管道操作，参看 {@link https://docs.mongodb.org/manual/reference/operator/aggregation-pipeline/}
     * @param {object[]?} pipeline - 聚合操作序列，比如[{$project:{author:1,tags:1}},{$unwind:"$tags"},{$group:{_id:{tags:"$tags"},authors:{$addToSet:"$author"}}}]，可以不填这个参数，则表示查询全部
     * @param {object?} options - 相关选项，比如{allowDiskUse:true}
     * @param {boolean?} [options.allowDiskUse=false] - 是否允许使用磁盘保存临时数据
     * @return {object[]} 结果文档数组，当使用$out命令时，返回值将是空数组
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    aggregateArray(pipeline, options) {
        return MyCollection._aggregateArrayCoroutine(this, "MyCollection~aggregateArray", false, pipeline, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 执行聚合操作，返回数组
     * 不要用这个函数查询大量数据，使用{@see aggregateCursor}
     * 对于管道操作，参看 {@link https://docs.mongodb.org/manual/reference/operator/aggregation-pipeline/}
     * @param {object[]?} pipeline - 聚合操作序列，比如[{$project:{author:1,tags:1}},{$unwind:"$tags"},{$group:{_id:{tags:"$tags"},authors:{$addToSet:"$author"}}}]，可以不填这个参数，则表示查询全部
     * @param {object?} options - 相关选项，比如{allowDiskUse:true}
     * @param {boolean?} [options.allowDiskUse=false] - 是否允许使用磁盘保存临时数据
     * @return {object[]} 结果文档数组，当使用$out命令时，返回值将是空数组
     */
    aggregateArrayNoThrow(pipeline, options) {
        return MyCollection._aggregateArrayCoroutine(this, "MyCollection~aggregateArrayNoThrow", true, pipeline, options);
    }

    /**
     * 执行聚合操作，直接返回MongoDB游标，没有使用协程，也没有加减延迟操作计数
     * 本函数使用场景受限，不要过多使用，一般用来长时间定时任务
     * 如果数据库连接无效，【不会重连数据库】，直接返回null
     * 对于迭代过程中，连接失效【也不会重连数据库】
     * 返回的Cursor可以如下迭代
     * @example
     *  //开始迭代
     *  do {
     *      var doc = yield cursor.next();
     *      //如果没有数据了，就返回null
     *      if (doc)
     *          //执行某些事
     *      else
     *          break;
     *  } while(true)
     *  //注意关闭
     *  cursor.close();
     * 对于管道操作，参看 {@link https://docs.mongodb.org/manual/reference/operator/aggregation-pipeline/}
     * @param {object[]?} pipeline - 聚合操作序列，比如[{$project:{author:1,tags:1}},{$unwind:"$tags"},{$group:{_id:{tags:"$tags"},authors:{$addToSet:"$author"}}}]，可以不填这个参数，则表示查询全部
     * @param {object?} options - 相关选项，比如{allowDiskUse:true}
     * @param {boolean?} [options.allowDiskUse=false] - 是否允许使用磁盘保存临时数据
     * @return {AggregationCursor|null} MongoDB的游标，当使用$out命令时，将迭代不到内容
     */
    aggregateCursor(pipeline, options) {
        if (!this._myDB)
            return null;

        if (!this._mongoCol)
            this._mongoCol = this._myDB.getMongoDB().collection(this._colName);

        return this._mongoCol.aggregate(pipeline, options);
    }

    /**
     * 查询数据符合条件的数据条数，返回整数
     * 如果出错会抛出异常
     * @param {object?} query - 查询条件，比如{a:1,b:1}，不填这个参数则表示查询所有
     * @param {object?} options - 相关选项，比如{skip:1,limit:1}
     * @param {number?} options.skip - 如果填了，就跳过多少条数据
     * @param {number?} options.limit - 如果填了，就限制多少条数据
     * @return {number} 数量
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    count(query, options) {
        return MyCollection._countCoroutine(this, "MyCollection~count", false, query, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 查询数据符合条件的数据条数，返回整数
     * @param {object?} query - 查询条件，比如{a:1,b:1}，不填这个参数则表示查询所有
     * @param {object?} options - 相关选项，比如{skip:1,limit:1}
     * @param {number?} options.skip - 如果填了，就跳过多少条数据
     * @param {number?} options.limit - 如果填了，就限制多少条数据
     * @return {number} 数量
     */
    countNoThrow(query, options) {
        return MyCollection._countCoroutine(this, "MyCollection~countNoThrow", true, query, options);
    }

    /**
     * 对一个字段进行去重，得到结果
     * 如果出错会抛出异常
     * @param {string} key - 用于去重的参考字段
     * @param {object?} query - 去重前排除一些数据
     * @return {object[]} 去重后的结果
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    distinct(key, query) {
        return MyCollection._distinctCoroutine(this, "MyCollection~distinct", false, key, query);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 对一个字段进行去重，得到结果
     * @param {string} key - 用于去重的参考字段
     * @param {object?} query - 去重前排除一些数据
     * @return {object[]} 去重后的结果
     */
    distinctNoThrow(key, query) {
        return MyCollection._distinctCoroutine(this, "MyCollection~distinctNoThrow", true, key, query);
    }

    /**
     * 创建索引，已存在的索引再创建不会引发错误
     * @param {object} fields - 索引的规格，比如{a:1,b:1}
     * @param {object?} options - 选项，比如{unique:true,background:true,name:"a_1_b_1"}
     * @param {boolean?} [options.unique=false] - 是否唯一索引
     * @param {boolean?} [options.background=false] - 是否后台创建，也就是不阻塞读写
     * @param {boolean?} [options.dropDups=false] - 是否删除重复的数据，当是唯一索引时。不过只在<3.0版时才有用，所以不建议用
     * @param {string?} options.name - 自定义的索引名，不填就会自动生成一个
     * @return {string} 索引名
     * @throws {Error} 可能的错误有：连接失败、唯一键重复等
     */
    createIndex(fields, options) {
        return MyCollection._createIndexCoroutine(this, "MyCollection~createIndex", false, fields, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 创建索引，已存在的索引再创建不会引发错误
     * @param {object} fields - 索引的规格，比如{a:1,b:1}
     * @param {object?} options - 选项，比如{unique:true,background:true,name:"a_1_b_1"}
     * @param {boolean?} [options.unique=false] - 是否唯一索引
     * @param {boolean?} [options.background=false] - 是否后台创建，也就是不阻塞读写
     * @param {boolean?} [options.dropDups=false] - 是否删除重复的数据，当是唯一索引时。不过只在<3.0版时才有用，所以不建议用
     * @param {string?} options.name - 自定义的索引名，不填就会自动生成一个
     * @return {string} 索引名
     */
    createIndexNoThrow(fields, options) {
        return MyCollection._createIndexCoroutine(this, "MyCollection~createIndexNoThrow", true, fields, options);
    }

    /**
     * 删除索引
     * @param {string} indexName - 索引名
     * @return {boolean} 是否成功删除
     * 实际上，索引不存在会引发错误，这里隐藏这个错误，存在就返回类似这样：{ nIndexesWas: 2, ok: 1 }，这里简化了
     * @throws {Error} 可能的错误有：连接失败等
     */
    dropIndex(indexName) {
        return MyCollection._dropIndexCoroutine(this, "MyCollection~dropIndex", false, indexName);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 删除索引
     * @param {string} indexName - 索引名
     * @return {boolean} 是否成功删除
     * 实际上，索引不存在会引发错误，这里隐藏这个错误，存在就返回类似这样：{ nIndexesWas: 2, ok: 1 }，这里简化了
     */
    dropIndexNoThrow(indexName) {
        return MyCollection._dropIndexCoroutine(this, "MyCollection~dropIndexNoThrow", true, indexName);
    }

    /**
     * 检测索引是否存在
     * @param {string|string[]} indexName - 索引名或索引名数组
     * @return {boolean} true表示（全部）索引存在，如果是多个索引检测，有一个不存在就返回false
     * @throws {Error} 可能的错误有：连接失败等
     */
    indexExists(indexName) {
        return MyCollection._indexExistsCoroutine(this, "MyCollection~indexExists", false, indexName);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 检测索引是否存在
     * @param {string|string[]} indexName - 索引名或索引名数组
     * @return {boolean} true表示（全部）索引存在，如果是多个索引检测，有一个不存在就返回false
     */
    indexExistsNoThrow(indexName) {
        return MyCollection._indexExistsCoroutine(this, "MyCollection~indexExistsNoThrow", true, indexName);
    }

    /**
     * 获取集合的所有索引信息
     * @return {object[]} 索引信息对象的数组，集合不存在时，返回空数组
     * 实际上，集合不存在会引发错误，这里隐藏这个错误
     * @throws {Error} 可能的错误有：连接失败等
     */
    indexes() {
        return MyCollection._indexesCoroutine(this, "MyCollection~indexes", false);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 获取集合的所有索引信息
     * @return {object[]} 索引信息对象的数组，集合不存在时，返回空数组
     */
    indexesNoThrow() {
        return MyCollection._indexesCoroutine(this, "MyCollection~indexesNoThrow", true);
    }

    /**
     * 删除多条数据
     * @param {object?} query - 匹配条件，比如{a:1,b:1}，不填这个参数则表示匹配所有
     * @return {number} 删除的条数，
     * 实际上，返回的如{ result: { ok: 1, n: 2 }, deletedCount: 2 }，这里简化了
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    deleteMany(query) {
        return MyCollection._deleteManyCoroutine(this, "MyCollection~deleteMany", false, query);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 删除多条数据
     * @param {object?} query - 匹配条件，比如{a:1,b:1}，不填这个参数则表示匹配所有
     * @return {number} 删除的条数，
     * 实际上，返回的如{ result: { ok: 1, n: 2 }, deletedCount: 2 }，这里简化了
     */
    deleteManyNoThrow(query) {
        return MyCollection._deleteManyCoroutine(this, "MyCollection~deleteManyNoThrow", true, query);
    }

    /**
     * 删除单条数据
     * @param {object?} query - 匹配条件，比如{a:1,b:1}，不填这个参数则表示匹配第一条
     * @return {number} 返回删除的数据行数
     * 实际上，返回的如{ result: { ok: 1, n: 1 }, deletedCount: 1 }，这里简化了
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    deleteOne(query) {
        return MyCollection._deleteOneCoroutine(this, "MyCollection~deleteOne", false, query);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 删除单条数据
     * @param {object?} query - 匹配条件，比如{a:1,b:1}，不填这个参数则表示匹配第一条
     * @return {number} 返回删除的数据行数
     * 实际上，返回的如{ result: { ok: 1, n: 1 }, deletedCount: 1 }，这里简化了
     */
    deleteOneNoThrow(query) {
        return MyCollection._deleteOneCoroutine(this, "MyCollection~deleteOneNoThrow", true, query);
    }

    /**
     * 查找一条数据，查到后返回这条数据，并删除它
     * @param {object} query - 匹配条件，比如{a:1,b:1}
     * @param {object?} options - 选项，比如{projection:{a:1,b:1},sort:{a:1,b:1}}
     * @param {object?} options.projection - 字段筛选，比如{a:1,b:1}
     * @param {object?} options.sort - 如果查询会得到多条数据，可以执行排序，然后返回和删除第一条，比如{a:1,b:1}
     * @return {object|null} 返回被删除的数据，如果没有查到数据，则返回null
     * 实际上，返回的如{lastErrorObject:{n:1},value:{_id:5674f6b130b7d391c0cfcdaa,a:1,b:1},ok:1}，如果有数据，value是非null，lastErrorObject.n为1，如果没有数据value为null，lastErrorObject.n为0，这里简化了
     * @throws {Error} 可能的错误有：连接失败、操作符不对等
     */
    findOneAndDelete(query, options) {
        return MyCollection._findOneAndDeleteCoroutine(this, "MyCollection~findOneAndDelete", false, query, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 查找一条数据，查到后返回这条数据，并删除它
     * @param {object} query - 匹配条件，比如{a:1,b:1}
     * @param {object?} options - 选项，比如{projection:{a:1,b:1},sort:{a:1,b:1}}
     * @param {object?} options.projection - 字段筛选，比如{a:1,b:1}
     * @param {object?} options.sort - 如果查询会得到多条数据，可以执行排序，然后返回和删除第一条，比如{a:1,b:1}
     * @return {object|null} 返回被删除的数据，如果没有查到数据，则返回null
     * 实际上，返回的如{lastErrorObject:{n:1},value:{_id:5674f6b130b7d391c0cfcdaa,a:1,b:1},ok:1}，如果有数据，value是非null，lastErrorObject.n为1，如果没有数据value为null，lastErrorObject.n为0，这里简化了
     */
    findOneAndDeleteNoThrow(query, options) {
        return MyCollection._findOneAndDeleteCoroutine(this, "MyCollection~findOneAndDeleteNoThrow", true, query, options);
    }

    /**
     * 查找一条数据，查到后，修改它，并返回原数据或新数据
     * @param {object} query - 匹配条件，比如{a:1,b:1}
     * @param {object} update - 要修改的数据，比如，如果只是修改，则用{$set:{a:1,b:1,c:1}}，如果要覆盖，则用{a:1,b:1,c:1}
     * @param {object?} options - 选项，比如{projection:{a:1,b:1},sort:{a:1,b:1}}
     * @param {object?} options.projection - 字段筛选，比如{a:1,b:1}
     * @param {object?} options.sort - 如果查询会得到多条数据，可以执行排序，然后返回和修改第一条，比如{a:1,b:1}
     * @param {boolean?} [options.upsert=false] - 如果查不到数据，是否插入
     * @param {boolean?} [options.returnOriginal=true] - 是否返回原数据，默认是
     * @return {object|null} 返回被修改的数据，根据不同选项，可能是修改前或修改后，如果没有数据，则返回null
     * 实际上，返回的以下，这里简化了
     * 如果原来没有数据，新插入，{lastErrorObject:{updatedExisting:false,n:1,upserted:5674fd54c89836681bd27227},value:null,ok:1}
     * 如果原来没有数据，不插入，{lastErrorObject:{updatedExisting:false,n:0},value:null,ok:1}
     * 如果原来有数据，{lastErrorObject:{updatedExisting:true,n:1},value:{_id:5674fd54c89836681bd27227,a:1,b:1},ok:1}
     * @throws {Error} 可能的错误有：连接失败、唯一键冲突、操作符不对等
     */
    findOneAndUpdate(query, update, options) {
        return MyCollection._findOneAndUpdateCoroutine(this, "MyCollection~findOneAndUpdate", false, query, update, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 查找一条数据，查到后，修改它，并返回原数据或新数据
     * @param {object} query - 匹配条件，比如{a:1,b:1}
     * @param {object} update - 要修改的数据，比如，如果只是修改，则用{$set:{a:1,b:1,c:1}}，如果要覆盖，则用{a:1,b:1,c:1}
     * @param {object?} options - 选项，比如{projection:{a:1,b:1},sort:{a:1,b:1}}
     * @param {object?} options.projection - 字段筛选，比如{a:1,b:1}
     * @param {object?} options.sort - 如果查询会得到多条数据，可以执行排序，然后返回和修改第一条，比如{a:1,b:1}
     * @param {boolean?} [options.upsert=false] - 如果查不到数据，是否插入
     * @param {boolean?} [options.returnOriginal=true] - 是否返回原数据，默认是
     * @return {object|null} 返回被修改的数据，根据不同选项，可能是修改前或修改后，如果没有数据，则返回null
     * 实际上，返回的以下，这里简化了
     * 如果原来没有数据，新插入，{lastErrorObject:{updatedExisting:false,n:1,upserted:5674fd54c89836681bd27227},value:null,ok:1}
     * 如果原来没有数据，不插入，{lastErrorObject:{updatedExisting:false,n:0},value:null,ok:1}
     * 如果原来有数据，{lastErrorObject:{updatedExisting:true,n:1},value:{_id:5674fd54c89836681bd27227,a:1,b:1},ok:1}
     */
    findOneAndUpdateNoThrow(query, update, options) {
        return MyCollection._findOneAndUpdateCoroutine(this, "MyCollection~findOneAndUpdateNoThrow", true, query, update, options);
    }

    /**
     * 插入多条数据
     * @param {object[]} docs - 要插入的数据
     * @return {number} 返回插入的数据行
     * 实际上，返回的如下，这里简化了
     * {
     *      result:{ok:1,n:4},
     *      ops:[{a:1,_id:5674fec0409cead81d9b4aa1},
     *          {a:2,_id:5674fec0409cead81d9b4aa2},
     *          {a:3,_id:5674fec0409cead81d9b4aa3},
     *          {a:4,_id:5674fec0409cead81d9b4aa4}],
     *      insertedCount:4,
     *      insertedIds:[5674fec0409cead81d9b4aa1,
     *          5674fec0409cead81d9b4aa2,
     *          5674fec0409cead81d9b4aa3,
     *          5674fec0409cead81d9b4aa4]
     * }
     * @throws {Error} 可能的错误有：连接失败、唯一键冲突、操作符不对等
     */
    insertMany(docs) {
        return MyCollection._insertManyCoroutine(this, "MyCollection~insertMany", false, docs);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 插入多条数据
     * @param {object[]} docs - 要插入的数据
     * @return {number} 返回插入的数据行
     * 实际上，返回的如下，这里简化了
     * {
     *      result:{ok:1,n:4},
     *      ops:[{a:1,_id:5674fec0409cead81d9b4aa1},
     *          {a:2,_id:5674fec0409cead81d9b4aa2},
     *          {a:3,_id:5674fec0409cead81d9b4aa3},
     *          {a:4,_id:5674fec0409cead81d9b4aa4}],
     *      insertedCount:4,
     *      insertedIds:[5674fec0409cead81d9b4aa1,
     *          5674fec0409cead81d9b4aa2,
     *          5674fec0409cead81d9b4aa3,
     *          5674fec0409cead81d9b4aa4]
     * }
     */
    insertManyNoThrow(docs) {
        return MyCollection._insertManyCoroutine(this, "MyCollection~insertManyNoThrow", true, docs);
    }

    /**
     * 插入一条数据
     * @param {object} doc - 要插入的数据
     * @return {number} 返回插入的数据行数
     * 实际上，返回的如{result:{ok:1,n:1},ops:[{a:1,_id:5674ffc12a7adab43a6b3b65}],insertedCount:1,insertedId:5674ffc12a7adab43a6b3b65}，这里简化了
     * @throws {Error} 可能的错误有：连接失败、唯一键冲突、操作符不对等
     */
    insertOne(doc) {
        return MyCollection._insertOneCoroutine(this, "MyCollection~insertOne", false, doc);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 插入一条数据
     * @param {object} doc - 要插入的数据
     * @return {number} 返回插入的数据行数
     * 实际上，返回的如{result:{ok:1,n:1},ops:[{a:1,_id:5674ffc12a7adab43a6b3b65}],insertedCount:1,insertedId:5674ffc12a7adab43a6b3b65}，这里简化了
     */
    insertOneNoThrow(doc) {
        return MyCollection._insertOneCoroutine(this, "MyCollection~insertOneNoThrow", true, doc);
    }

    /**
     * 更新多条数据
     * @param {object} query - 匹配要更新的数据
     * @param {object} update - 要执行的更新操作，这里不能是{a:1,b:1}这样覆盖式的操作，必须是带$的操作，否则触发错误
     * @param {object?} options - 选项，比如{upsert:true}
     * @param {boolean?} [options.upsert=false] - 如果查不到数据，是否插入
     * @return {number} 返回匹配的数据行，有可能有些数据改前后都一样，所以实际修改的数据行不一定等于这个值
     * 实际上，返回的是如{result:{ok:1,nModified:0,n:1,upserted:[[Object]]},matchedCount:1,modifiedCount:0,upsertedId:{index:0,_id:567501a1c89836681bd27229},upsertedCount:1}，这里简化了
     * @throws {Error} 可能的错误有：连接失败、唯一键冲突、操作符不对等
     */
    updateMany(query, update, options) {
        return MyCollection._updateManyCoroutine(this, "MyCollection~updateMany", false, query, update, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 更新多条数据
     * @param {object} query - 匹配要更新的数据
     * @param {object} update - 要执行的更新操作，这里不能是{a:1,b:1}这样覆盖式的操作，必须是带$的操作，否则触发错误
     * @param {object?} options - 选项，比如{upsert:true}
     * @param {boolean?} [options.upsert=false] - 如果查不到数据，是否插入
     * @return {number} 返回匹配的数据行，有可能有些数据改前后都一样，所以实际修改的数据行不一定等于这个值
     * 实际上，返回的是如{result:{ok:1,nModified:0,n:1,upserted:[[Object]]},matchedCount:1,modifiedCount:0,upsertedId:{index:0,_id:567501a1c89836681bd27229},upsertedCount:1}，这里简化了
     */
    updateManyNoThrow(query, update, options) {
        return MyCollection._updateManyCoroutine(this, "MyCollection~updateManyNoThrow", true, query, update, options);
    }

    /**
     * 更新单条数据
     * @param {object} query - 匹配要更新的数据
     * @param {object} update - 要执行的更新操作，这里单条，可以是覆盖式操作{a:1,b:1}也可以是修改式操作{$set:{a:1,b:1}}
     * @param {object?} options - 选项，比如{upsert:true}
     * @param {boolean?} [options.upsert=false] - 如果查不到数据，是否插入
     * @return {number} 返回匹配的数据行，有可能有些数据改前后都一样，所以实际修改的数据行不一定等于这个值
     * 实际上，返回的是如{result:{ok:1,nModified:0,n:1,upserted:[[Object]]},matchedCount:1,modifiedCount:0,upsertedId:{index:0,_id:567502aec89836681bd2722a},upsertedCount:1}，这里简化了
     * @throws {Error} 可能的错误有：连接失败、唯一键冲突、操作符不对等
     */
    updateOne(query, update, options) {
        return MyCollection._updateOneCoroutine(this, "MyCollection~updateOne", false, query, update, options);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 更新单条数据
     * @param {object} query - 匹配要更新的数据
     * @param {object} update - 要执行的更新操作，这里单条，可以是覆盖式操作{a:1,b:1}也可以是修改式操作{$set:{a:1,b:1}}
     * @param {object?} options - 选项，比如{upsert:true}
     * @param {boolean?} [options.upsert=false] - 如果查不到数据，是否插入
     * @return {number} 返回匹配的数据行，有可能有些数据改前后都一样，所以实际修改的数据行不一定等于这个值
     * 实际上，返回的是如{result:{ok:1,nModified:0,n:1,upserted:[[Object]]},matchedCount:1,modifiedCount:0,upsertedId:{index:0,_id:567502aec89836681bd2722a},upsertedCount:1}，这里简化了
     */
    updateOneNoThrow(query, update, options) {
        return MyCollection._updateOneCoroutine(this, "MyCollection~updateOneNoThrow", true, query, update, options);
    }

    /**
     * 删除集合
     * 集合不存在会抛出错误，但这里隐藏这个错误了
     * @return {boolean} 是否删除了集合
     * @throws {Error} 可能的错误有：连接失败等
     */
    drop() {
        return MyCollection._dropCoroutine(this, "MyCollection~drop", false);
    }

    /**
     * 注：NoThrow函数就是指不会抛出错误，一般还是要捕获错误，所以少用
     * 删除集合
     * @return {boolean} 是否删除了集合
     */
    dropNoThrow() {
        return MyCollection._dropCoroutine(this, "MyCollection~dropNoThrow", true);
    }
}

MyCollection._findArrayCoroutine = Promise.coroutine(function * (self, func, noThrow, query, fields, options, debug) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.find(query, fields, options).toArray();
                if (debug)
                    logUtil.debug(ret);
                if (options && (options.limit === 1 && options.returnObject))
                    return ret[0] || null;
                else
                    return ret;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._aggregateArrayCoroutine = Promise.coroutine(function * (self, func, noThrow, pipeline, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //构造
                var cursor = self._mongoCol.aggregate(pipeline || [], options);
                //使用协程执行
                return yield cursor.toArray();
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._countCoroutine = Promise.coroutine(function * (self, func, noThrow, query, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                return yield self._mongoCol.count(query, options);
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._distinctCoroutine = Promise.coroutine(function * (self, func, noThrow, key, query) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                return yield self._mongoCol.distinct(key, query);
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._createIndexCoroutine = Promise.coroutine(function * (self, func, noThrow, fields, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                return yield self._mongoCol.createIndex(fields, options);
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._dropIndexCoroutine = Promise.coroutine(function * (self, func, noThrow, indexName) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.dropIndex(indexName);
                if (!ret || !ret.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }
                return !!(ret && ret.ok);
            }
            catch (err) {
                if (err.code === ERROR_CODE.IndexNotFound) {
                    return false;
                }
                else {
                    logUtil.warn("数据库错误，函数：" + func, err);
                    errObj = err;
                }
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._indexExistsCoroutine = Promise.coroutine(function * (self, func, noThrow, indexName) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                return yield self._mongoCol.indexExists(indexName);
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._indexesCoroutine = Promise.coroutine(function * (self, func, noThrow) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                return yield self._mongoCol.indexes();
            }
            catch (err) {
                //集合不存在不算错误
                if (err.code === ERROR_CODE.NamespaceNotFound) {
                    return [];
                }
                else {
                    logUtil.warn("数据库错误，函数：" + func, err);
                    errObj = err;
                }
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._deleteManyCoroutine = Promise.coroutine(function * (self, func, noThrow, query) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.deleteMany(query);
                if (!ret || !ret.result || !ret.result.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }

                return ret && ret.result ? Math.floor(ret.result.n) : 0;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._deleteOneCoroutine = Promise.coroutine(function * (self, func, noThrow, query) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.deleteOne(query);
                if (!ret || !ret.result || !ret.result.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }

                return ret && ret.result ? Math.floor(ret.result.n) : 0;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._findOneAndDeleteCoroutine = Promise.coroutine(function * (self, func, noThrow, query, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.findOneAndDelete(query, options);
                if (!ret || !ret.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }
                return ret ? ret.value : null;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._findOneAndUpdateCoroutine = Promise.coroutine(function * (self, func, noThrow, query, update, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.findOneAndUpdate(query, update, options);
                if (!ret || !ret.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }
                return ret ? ret.value : null;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._insertManyCoroutine = Promise.coroutine(function * (self, func, noThrow, docs) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.insertMany(docs);
                if (!ret || !ret.result || !ret.result.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }

                return ret && ret.result ? Math.floor(ret.result.n) : 0;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._insertOneCoroutine = Promise.coroutine(function * (self, func, noThrow, doc) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.insertOne(doc);
                if (!ret || !ret.result || !ret.result.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }

                return ret && ret.result ? Math.floor(ret.result.n) : 0;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._updateManyCoroutine = Promise.coroutine(function * (self, func, noThrow, query, update, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.updateMany(query, update, options);
                if (!ret || !ret.result || !ret.result.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }

                return ret && ret.result ? Math.floor(ret.result.n) : 0;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._updateOneCoroutine = Promise.coroutine(function * (self, func, noThrow, query, update, options) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                var ret = yield self._mongoCol.updateOne(query, update, options);
                if (!ret || !ret.result || !ret.result.ok) {
                    logUtil.warn("数据库返回错误消息，函数：" + func + ", 返回数据：" + ret);
                }

                return ret && ret.result ? Math.floor(ret.result.n) : 0;
            }
            catch (err) {
                logUtil.warn("数据库错误，函数：" + func, err);
                errObj = err;
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});

MyCollection._dropCoroutine = Promise.coroutine(function * (self, func, noThrow) {
    try {
        //延迟操作计数加一
        MyDB.incPendingOp();
        var errObj = null;
        do {
            /**
             * 先检查连接，一开始errObj为null，不会强制检测连接是否有效。
             * 为了性能，第一次如果连接有效，就不用协程检查连接了
             * @type {boolean} true没发生重连，false表示发生过重连
             */
            var noReconn = errObj === null && self._myDB.isConnect() ? true : yield self._myDB.checkConn(true);

            /*
             * 当发生过错误时，就循环回到这里，强制检测连接。
             * 如果未发生过重连，这说明这种错误可能是参数错误导致的，那就抛出异常。
             * 如果发生过重连，这说明这种错误可能是连接失效导致的，继续后面的操作。
             */
            if (errObj && noReconn)
                throw errObj;

            //要重新开始了，错误对象重置
            errObj = null;

            try {
                //获取数据库连接对象，正常情况下不会抛出错误，但为了保险，还是放到try里
                if (!self._mongoCol)
                    self._mongoCol = self._myDB.getMongoDB().collection(self._colName);

                //使用协程执行
                return yield self._mongoCol.drop();
            }
            catch (err) {
                //集合不存在不算错误
                if (err.code === ERROR_CODE.NamespaceNotFound) {
                    return false;
                }
                else {
                    logUtil.warn("数据库错误，函数：" + func, err);
                    errObj = err;
                }
            }
        }
        while (errObj);
    }
    catch (err) {
        if (!noThrow)
            throw err;
    }
    finally {
        //延迟操作计数减一
        MyDB.decPendingOp();
    }
});
////////////导出函数////////////
var initDBCoroutine = Promise.coroutine(function * () {
    if (myDBPool.length <= 0) {
        var poolSize = Math.min(16, Math.max(1, appCfg.dbPoolSize));
        for (var i = 0; i < poolSize; ++i) {
            var db = new MyDB(i, appCfg.dbUrl);
            myDBPool[i] = db;
            try
            {
                yield db.connect();
            }
            catch (err)
            {
                //关闭现有的
                yield closeDB();
                //抛出错误
                throw err;
            }
        }
    }
});

/**
 * 用协程初始化，如果初始化失败，会触发错误，并会把已建立的连接关闭
 * @throws {Error}
 */
function initDB() {
    return initDBCoroutine();
}

/**
 * 返回包装过的数据库对象
 * @param {(number|string)?} id - 用于计算采用哪条的连接ID数值，不填就使用随机
 * @returns {MyDB|null} 如果没有连接可用，就返回null
 */
function getDB(id) {
    if (myDBPool.length <= 0)
        return null;

    var index;
    if (Object.isUndefined(id))
        index = appUtil.getRandom(0, myDBPool.length - 1);
    else if (Object.isString(id))
        index = Math.abs(id.hashCode()) % myDBPool.length;
    else
        index = (id >>> 0) % myDBPool.length;
    return myDBPool[index];
}

var closeDBCoroutine = Promise.coroutine(function * () {
    //等所有数据库连接完成读写
    while (MyDB.getPendingOpCnt() > 0)
        yield Promise.delay(10);
    for (var i = 0; i < myDBPool.length; ++i) {
        var db = myDBPool[i];
        try {
            db.close(true);
        }
        catch (err) {
            logUtil.error(null, err);
        }
    }
    myDBPool = [];
});

/**
 * 用协程关闭数据库连接，并清除对象
 */
function closeDB() {
    return closeDBCoroutine();
}

function incPendingOp() {
    MyDB.incPendingOp();
}

function decPendingOp() {
    MyDB.decPendingOp();
}

function getPendingOp() {
    return MyDB.getPendingOpCnt();
}

exports.ERROR_CODE = ERROR_CODE;
exports.initDB = initDB;
exports.getDB = getDB;
exports.closeDB = closeDB;
exports.incPendingOp = incPendingOp;
exports.decPendingOp = decPendingOp;
exports.getPendingOp = getPendingOp;
exports.MongoError = MongoDB.MongoError;