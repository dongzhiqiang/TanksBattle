"use strict";

var gameConfig = require("./gameConfig");

let maxElement = 4;
class ElementConfig
{
    constructor() {
        this.weaponId = 0;
        this.elementId = 0;
        this.icon="";
    }

    static fieldsDesc() {
        return {
            weaponId: {type: Number},
            elementId: {type: Number},
            icon: {type: String},
        };
    }

    static getMaxElement(){
        return maxElement;
    }
}

module.exports = ElementConfig;