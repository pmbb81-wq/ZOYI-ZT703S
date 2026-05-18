function parseLabelValueSuffix_LUA(buff)
    local label_value = {}
    for part in string.gmatch(buff, "([^:]+)") do
        table.insert(label_value, part)
    end

    local ret = {"", label_value[2], ""}

    if label_value[1] == "Electricity" then
        ret[1] = "POMIAR PRĄDU"
        ret[3] = ret[3] .. "A"
    elseif label_value[1] == "AElectricity" then
        ret[1] = "POMIAR PRĄDU"
        ret[3] = "A"
    elseif label_value[1] == "mAElectricity" then
        ret[1] = "POMIAR PRĄDU"
        ret[3] = "mA"
    elseif label_value[1] == "MOMResistance" then
        ret[1] = "POMIAR OPORNOŚCI"
        ret[3] = "MΩ"
    elseif label_value[1] == "OMResistance" then
        ret[1] = "POMIAR OPORNOŚCI"
        ret[3] = "Ω"
    elseif label_value[1] == "KOMResistance" then
        ret[1] = "POMIAR OPORNOŚCI"
        ret[3] = "KΩ"
    elseif label_value[1] == "OMbeep" then
        ret[1] = "CIĄGŁOŚĆ"
        ret[3] = ""
    elseif label_value[1] == "VDiode" then
        ret[1] = "DIODA"
        ret[3] = "mV"
        if not string.find(ret[2], "0,") then
            ret[3] = "V"
        end
    elseif label_value[1] == "nFCap" then
        ret[1] = "POMIAR POJEMNOŚCI"
        ret[3] = "nF"
    elseif label_value[1] == "uFCap" then
        ret[1] = "POMIAR POJEMNOŚCI"
        ret[3] = "uF"
    elseif label_value[1] == "mFCap" then
        ret[1] = "POMIAR POJEMNOŚCI"
        ret[3] = "mF"
    elseif label_value[1] == "VVoltage" then
        ret[1] = "POMIAR NAPIĘCIA"
        ret[3] = "V"
    elseif label_value[1] == "mVVoltage" then
        ret[1] = "POMIAR NAPIĘCIA"
        ret[3] = "mV"
    else
        ret[1] = label_value[1]
        ret[3] = ""
    end

    return ret
end
