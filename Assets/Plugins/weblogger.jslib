var fileText = undefined;
var waitingForText = false;

mergeInto(LibraryManager.library, {
    WriteFile : function (name, text) {
        var element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', name);

        element.style.display = 'none';
        document.body.appendChild(element);

        element.click();

        document.body.removeChild(element);
    },

    ReadFile : function () {
        var input = document.createElement('input');
        input.type = 'file';

        input.onchange = e => {
            var file = e.target.files[0]; 

            var reader = new FileReader();
            reader.readAsText(file, 'UTF-8');
            console.log("Reading the input");

            reader.onload = readerEvent => {
                fileText = readerEvent.target.result;
                console.log("Reading the text" + fileText);
                waitingForText = false;
            }
        }
        console.log("Clicking the input");


        input.click();
        fileText = undefined;
        waitingForText = true;
    },

    IsWaitingForText : function () {
        return waitingForText;
    },

    GetText : function () {

        let buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(fileText, buffer);

        return buffer;
    }
});