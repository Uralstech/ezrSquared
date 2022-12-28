using ezrSquared.Errors;
using ezrSquared.Nodes;
using ezrSquared.Values;

namespace ezrSquared.Helpers
{
    public class parseResult
    {
        public error? error;
        public node? node;
        public int advanceCount;
        public int reverseCount;

        public parseResult()
        {
            this.error = null;
            this.node = null;
            this.advanceCount = 0;
            this.reverseCount = 0;
        }

        public void registerAdvance() { advanceCount++; }

        public node register(parseResult result)
        {
            advanceCount += result.advanceCount;
            if (result.error != null)
                error = result.error;
            return result.node;
        }

        public node? tryRegister(parseResult result)
        {
            if (result.error != null)
            {
                reverseCount = result.advanceCount;
                return null;
            }

            return register(result);
        }

        public parseResult success(node node)
        {
            this.node = node;
            return this;
        }

        public parseResult failure(error error)
        {
            if (this.error == null || advanceCount == 0)
                this.error = error;
            return this;
        }
    }

    public class runtimeResult
    {
        public item? value;
        public error? error;
        public item? functionReturnValue;
        public bool loopShouldSkip;
        public bool loopShouldStop;

        public runtimeResult() { reset(); }

        public void reset()
        {
            value = null;
            error = null;
            functionReturnValue = null;
            loopShouldSkip = false;
            loopShouldStop = false;
        }

        public item? register(runtimeResult result)
        {
            if (result.error != null)
                error = result.error;
            functionReturnValue = result.functionReturnValue;
            loopShouldSkip = result.loopShouldSkip;
            loopShouldStop = result.loopShouldStop;
            return result.value;
        }

        public runtimeResult success(item value)
        {
            reset();
            this.value = value;
            return this;
        }

        public runtimeResult returnSuccess(item value)
        {
            reset();
            functionReturnValue = value;
            return this;
        }

        public runtimeResult skipSuccess()
        {
            reset();
            loopShouldSkip = true;
            return this;
        }

        public runtimeResult stopSuccess()
        {
            reset();
            loopShouldStop = true;
            return this;
        }

        public runtimeResult failure(error error)
        {
            reset();
            this.error = error;
            return this;
        }

        public bool shouldReturn() { return (error != null || functionReturnValue != null || loopShouldSkip || loopShouldStop); }
    }
}