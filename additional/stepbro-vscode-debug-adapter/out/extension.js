"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
const vscode = require("vscode");
// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
function activate(context) {
    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.getProgramName', config => {
        return vscode.window.showInputBox({
            placeHolder: "Please enter the name of a stepbro file in the workspace folder",
            value: "ConsoleTest.sbs"
        });
    }));
    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.startSession', config => startSession(config)));
}
exports.activate = activate;
// This method is called when your extension is deactivated
function deactivate() { }
exports.deactivate = deactivate;
function startSession(config) {
    console.log("We started a session!");
    vscode.commands.executeCommand('vscode.startDebug', config);
}
//# sourceMappingURL=extension.js.map