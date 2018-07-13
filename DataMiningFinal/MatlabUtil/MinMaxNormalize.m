function NormalizedData = MinMaxNormalize(data) 
    mindata = min(data);
    maxdata = max(data);
    NormalizedData = bsxfun(@rdivide, bsxfun(@minus, data, mindata), maxdata - mindata);
end