﻿// Constants for RECEIPT NOTE
var RECEIPT_NOTE_CODE_CELL_INDEX = 2;
var RECEIPT_NOTE_NAME_CELL_INDEX = 3;
var RECEIPT_NOTE_UNIT_CELL_INDEX = 4;
var RECEIPT_NOTE_QUANTITY_CELL_INDEX = 5;
var RECEIPT_NOTE_PRICE_CELL_INDEX = 6;


// Constants for DELIVERY NOTE

// Filter type
var FILTER_ALL = 0;
var FILTER_DATE_RANGE = 1;

// HTTP Status Code
var HTTP_STATUS_CODE_OK = 'OK';

// Date time
var DEFAULT_DATE_PICKER_FORMAT = 'dd/mm/yyyy';
var DEFAULT_MOMENT_DATE_FORMAT = 'DD/MM/YYYY';
var DEFAULT_MOMENT_DATE_TIME_FORMAT = 'DD/MM/YYYY hh:mm:ss';
var MIN_PRODUCTION_DATA_DATE = new Date(2010, 1 - 1, 1);

// Search Type
var SEARCH_TYPE_NONE = 0;
var SEARCH_TYPE_DRUG = 1;
var SEARCH_TYPE_STAFF = 2;
var SEARCH_TYPE_CUSTOMER = 3;
var SEARCH_TYPE_SUPPLYER = 4;

// Report Group Filter
var REPORT_FILTER_TYPE_ALL = 0;
var REPORT_FILTER_TYPE_BY_GROUP = 1;
var REPORT_FILTER_TYPE_BY_NAME = 2;

// Item Type Filter
var ITEM_FILTER_TYPE_NONE = -1;
var ITEM_FILTER_TYPE_BY_DRUG_GROUP = 0;
var ITEM_FILTER_TYPE_BY_DRUG = 1;
var ITEM_FILTER_TYPE_BY_CUSTOMER_GROUP = 2;
var ITEM_FILTER_TYPE_BY_CUSTOMER = 3;
var ITEM_FILTER_TYPE_BY_SUPPLYER_GROUP = 4;
var ITEM_FILTER_TYPE_BY_SUPPLYER = 5;
var ITEM_FILTER_TYPE_BY_STAFF_GROUP = 6;
var ITEM_FILTER_TYPE_BY_STAFF = 7;
var ITEM_FILTER_TYPE_BY_DOCTOR_GROUP = 8;
var ITEM_FILTER_TYPE_BY_DOCTOR = 9;

// Report By Type Id
var REPORT_BY_TYPE_STAFF = 0;
var REPORT_BY_TYPE_CUSTOMER = 1;
var REPORT_BY_TYPE_DOCTOR = 2;
var REPORT_BY_TYPE_SUPPLYER = 3;
var REPORT_BY_TYPE_GOODS = 4;
var REPORT_BY_TYPE_GOODS_BY_DOCTOR = 5;

// Common
var ESP_AMOUNT = 0.1;

// Task Mode
var TASK_VIEW_MODE = 0;
var TASK_CREATE_MODE_ = 1;
var TASK_EDIT_MODE = 2;
var TASK_DELETE_MODE = 3;