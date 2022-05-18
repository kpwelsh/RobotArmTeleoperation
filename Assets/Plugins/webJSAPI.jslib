var jsAPILib={
    ReadRecord: async function(taskPtr, callbackPtr, namePtr) {
        function getPtrFromString(str) {
            var len = lengthBytesUTF8(str) + 1;
            var strPtr = _malloc(len);
            stringToUTF8(str, strPtr, len);
            return strPtr;
        }
        let name = UTF8ToString(namePtr);
        Module['dynCall_vii'](callbackPtr, taskPtr, getPtrFromString(name));
    },

    WriteRecord: async function(taskPtr, callbackPtr, namePtr, recordPtr) {
        let name = UTF8ToString(namePtr);
        let record = UTF8ToString(recordPtr);

        Module['dynCall_vi'](callbackPtr, taskPtr);
    },

    ListRecords: async function(taskPtr, callbackPtr) {
        Module['dynCall_vii'](callbackPtr, taskPtr, ["record 1", "record 2"]);
    }
};
mergeInto(LibraryManager.library, jsAPILib);