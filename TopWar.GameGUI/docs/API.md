    case "HEARTBEAT":
        await writer.WriteLineAsync("OK");
        break;
    case "OCR":
        await HandleOcrRequestAsync(jsonNode, writer);
        break;
    case "SCREENSHOT32BIT":
        await HandleScreenShot32RequestAsync(jsonNode, writer);
        break;
    case "SCREENSHOT24BIT":
        await HandleScreenShot24RequestAsync(jsonNode, writer);
        break;
    case "CLICK":
        await HandleClickRequestAsync(jsonNode, writer);
        break;
    case "MOVE":
        await HandleMoveRequestAsync(jsonNode, writer);
        break;
    case "SCROOL":
        await HandleScroolRequestAsync(jsonNode, writer);
        break;
    case "KEYPRESS":
        await HandleKeyPressRequestAsync(jsonNode, writer);
        break;
    case "HANDLE":
        IntPtr handle = ChromiumWebBrowserIns.GetWindowHandle();
        await writer.WriteLineAsync($"{handle}");
        break;
    default:
        await writer.WriteLineAsync("´íÎó£ºÎ´ÖªÃüÁî");
        break;